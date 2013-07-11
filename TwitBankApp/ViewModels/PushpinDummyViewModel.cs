using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yandex.Maps;

namespace TwitBankApp.ViewModels
{
    public class PushpinDummyViewModel
    {
        public Visibility Visibility { get; set; }
        public Visibility ContentVisibility { get; set; }
        public PushPinState State { get; set; }
        public Yandex.Positioning.GeoCoordinate Position { get; set; }
        public int ZIndex { get; set; }
        public string Text { get; set; }
        public string empty { get; set; }
    }
}
