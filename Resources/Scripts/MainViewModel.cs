using Lessop.Resources.Strings;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lessop
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<OptionItem> Options { get; set; } = new()
    {
        new OptionItem {Name = AppResources.Kyiv, Tag = "34"}, //Вибір district / Choose district
        new OptionItem {Name = AppResources.Kyiv_Bil, Tag = "73"},
        new OptionItem {Name = AppResources.Kyiv_bor, Tag = "78"},
        new OptionItem {Name = AppResources.Kyiv_brov, Tag = "79"},
        new OptionItem {Name = AppResources.Kyiv_Buch, Tag = "75"},
        new OptionItem {Name = AppResources.Kyiv_Fast, Tag = "77"},
        new OptionItem {Name = AppResources.Kyiv_Obu, Tag = "76"},
        new OptionItem {Name = AppResources.Kyiv_Vysh, Tag = "74"},
        new OptionItem {Name = AppResources.Sum_Kon, Tag = "117"},
        new OptionItem {Name = AppResources.Sum_Okht, Tag = "118"},
        new OptionItem {Name = AppResources.Sum_Rom, Tag = "116"},
        new OptionItem {Name = AppResources.Sum_Sho, Tag = "115"},
        new OptionItem {Name = AppResources.Sum_Sum, Tag = "114"},
        new OptionItem {Name = AppResources.Cher_Cher, Tag = "140"},
        new OptionItem {Name = AppResources.Cher_Kor, Tag = "144"},
        new OptionItem {Name = AppResources.Cher_Niz, Tag = "142"},
        new OptionItem {Name = AppResources.Cher_Nov, Tag = "141"},
        new OptionItem {Name = AppResources.Cher_Pru, Tag = "143"}
    };


        private OptionItem _selectedOption = null!;
        public OptionItem SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged();
                }
            }
        }
        public MainViewModel(string regionTag)
        {
            SelectedOption = Options.FirstOrDefault(o => o.Tag == regionTag) ?? Options.First();
        }

    }


}
