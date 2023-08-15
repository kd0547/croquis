using System.ComponentModel;

public class MainViewModel : INotifyPropertyChanged
{
    #region event
    public event PropertyChangedEventHandler? PropertyChanged;
    #endregion

    public MainViewModel()
    {
    }


}
