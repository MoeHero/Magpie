namespace com.moehero.cuckoo.Code
{
    internal interface IFunction
    {
        string Description { get; }

        bool Handled { get; set; }

        bool CanRun();

        void Run();
    }
}
