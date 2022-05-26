/*  CTRADER GURU --> Template 1.0.6

    Homepage    : https://ctrader.guru/
    Telegram    : https://t.me/ctraderguru
    Twitter     : https://twitter.com/cTraderGURU/
    Facebook    : https://www.facebook.com/ctrader.guru/
    YouTube     : https://www.youtube.com/channel/UCKkgbw09Fifj65W5t5lHeCQ
    GitHub      : https://github.com/cTraderGURU/

*/

using System;
using cAlgo.API;
using System.Threading;
using cAlgo.API.Indicators;
using System.Windows.Forms;

namespace cAlgo.Indicators
{

    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class Supertrend : Indicator
    {

        #region Identity

        public const string NAME = "Supertrend";

        public const string VERSION = "1.0.5";

        #endregion

        #region Params

        [Parameter(NAME + " " + VERSION, Group = "Identity", DefaultValue = "https://www.google.com/search?q=ctrader+guru+supertrend")]
        public string ProductInfo { get; set; }

        [Parameter(DefaultValue = 10, Group = "Params")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 3.0, Group = "Params")]
        public double Multiplier { get; set; }

        [Parameter("Alert On Change ?", Group = "Params", DefaultValue = false)]
        public bool AlertOnChange { get; set; }

        [Output("UpTrend", LineColor = "Green", PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries UpTrend { get; set; }

        [Output("DownTrend", LineColor = "Red", PlotType = PlotType.Points, Thickness = 3)]
        public IndicatorDataSeries DownTrend { get; set; }

        #endregion

        #region Property

        private IndicatorDataSeries _upBuffer;
        private IndicatorDataSeries _downBuffer;
        private AverageTrueRange _averageTrueRange;
        private int[] _trend;
        private bool _changeofTrend;
        int lastDirection = 0;

        int LastIndex = -1;
        bool AlertInThisBar = true;

        #endregion

        #region Indicator Events

        protected override void Initialize()
        {

            Print("{0} : {1}", NAME, VERSION);

            _trend = new int[1];
            _upBuffer = CreateDataSeries();
            _downBuffer = CreateDataSeries();
            _averageTrueRange = Indicators.AverageTrueRange(Period, MovingAverageType.WilderSmoothing);

        }

        public override void Calculate(int index)
        {

            if (index != LastIndex)
            {

                if (LastIndex != -1)
                    AlertInThisBar = false;
                LastIndex = index;

            }

            UpTrend[index] = double.NaN;
            DownTrend[index] = double.NaN;

            double median = (Bars.HighPrices[index] + Bars.LowPrices[index]) / 2;
            double atr = _averageTrueRange.Result[index];

            _upBuffer[index] = median + Multiplier * atr;
            _downBuffer[index] = median - Multiplier * atr;


            if (index < 1)
            {
                _trend[index] = 1;
                return;
            }

            Array.Resize(ref _trend, _trend.Length + 1);


            if (Bars.ClosePrices[index] > _upBuffer[index - 1])
            {
                _trend[index] = 1;
                if (_trend[index - 1] == -1)
                    _changeofTrend = true;
            }
            else if (Bars.ClosePrices[index] < _downBuffer[index - 1])
            {
                _trend[index] = -1;
                if (_trend[index - 1] == -1)
                    _changeofTrend = true;
            }
            else if (_trend[index - 1] == 1)
            {
                _trend[index] = 1;
                _changeofTrend = false;
            }
            else if (_trend[index - 1] == -1)
            {
                _trend[index] = -1;
                _changeofTrend = false;
            }

            if (_trend[index] < 0 && _trend[index - 1] > 0)
                _upBuffer[index] = median + (Multiplier * atr);
            else if (_trend[index] < 0 && _upBuffer[index] > _upBuffer[index - 1])
                _upBuffer[index] = _upBuffer[index - 1];

            if (_trend[index] > 0 && _trend[index - 1] < 0)
                _downBuffer[index] = median - (Multiplier * atr);
            else if (_trend[index] > 0 && _downBuffer[index] < _downBuffer[index - 1])
                _downBuffer[index] = _downBuffer[index - 1];

            if (_trend[index] == 1)
            {
                UpTrend[index] = _downBuffer[index];
                if (_changeofTrend)
                {

                    UpTrend[index - 1] = DownTrend[index - 1];
                    _changeofTrend = false;
                }
            }
            else if (_trend[index] == -1)
            {
                DownTrend[index] = _upBuffer[index];
                if (_changeofTrend)
                {

                    DownTrend[index - 1] = UpTrend[index - 1];
                    _changeofTrend = false;
                }

            }

            IsChanged(index);

        }

        #endregion

        #region Private Methods

        void IsChanged(int myindex)
        {

            if (!IsLastBar || !AlertOnChange || AlertInThisBar)
                return;

            if (lastDirection > -1 && DownTrend[myindex] > 0)
            {

                if (lastDirection == 0)
                {

                    lastDirection = -1;
                    return;

                }

                lastDirection = -1;
                AlertInThisBar = true;
                new Thread(new ThreadStart(delegate { MessageBox.Show(string.Format("{0} {1} {2} changed to DOWN", NAME, Symbol.Name, Bars.TimeFrame), "Changed", MessageBoxButtons.OK, MessageBoxIcon.Information); })).Start();

            }
            else if (lastDirection < 1 && UpTrend[myindex] > 0)
            {

                if (lastDirection == 0)
                {

                    lastDirection = 1;
                    return;

                }

                lastDirection = 1;
                AlertInThisBar = true;
                new Thread(new ThreadStart(delegate { MessageBox.Show(string.Format("{0} {1} {2} changed to UP", NAME, Symbol.Name, Bars.TimeFrame), "Changed", MessageBoxButtons.OK, MessageBoxIcon.Information); })).Start();

            }

        }

        #endregion


    }

}
