using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace PiCameraSurveillanceClient.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
            SimpleIoc.Default.Register<ConfigViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public AboutViewModel About => ServiceLocator.Current.GetInstance<AboutViewModel>();

        public ConfigViewModel ConfigView => ServiceLocator.Current.GetInstance<ConfigViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}