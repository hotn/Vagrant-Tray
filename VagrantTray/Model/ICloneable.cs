namespace MikeWaltonWeb.VagrantTray.Model
{
    /// <summary>
    /// Interface for deep cloning objects that returns explicitly typed clones.
    /// </summary>
    /// <typeparam name="T">Type of object being cloned</typeparam>
    public interface ICloneable<T>
    {
        T Clone();
    }
}
