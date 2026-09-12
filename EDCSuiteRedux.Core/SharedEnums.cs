namespace EDCSuiteRedux
{
    public enum CarMakes : int
    {
        Audi,
        BMW,
        Volkswagen,
        Unknown
    }

    public enum GearboxType : int
    {
        Automatic,
        Manual,
        FourWheelDrive
    }

    public enum EDCFileType : int
    {
        EDC15P,
        EDC15P6,
        EDC15V,
        EDC15M,
        EDC15C,
        EDC16,
        EDC17,
        MSA6,
        MSA11,
        MSA12,
        MSA15,
        Unknown
    }

    public enum EngineType : int
    {
        cc1200,
        cc1400,
        cc1600,
        cc1900,
        cc2500
    }

    public enum ImportFileType : int
    {
        XML,
        A2L,
        CSV,
        AS2,
        Damos
    }
}