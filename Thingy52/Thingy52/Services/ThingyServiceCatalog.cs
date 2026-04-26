namespace Thingy52;

internal static class ThingyServiceCatalog
{
    // Services
    public const string UiServiceUuid     = "ef680300-9b35-4933-9b10-52ffa9740042";
    public const string MotionServiceUuid = "ef680400-9b35-4933-9b10-52ffa9740042";
    public const string SoundServiceUuid  = "ef680500-9b35-4933-9b10-52ffa9740042";

    // UI Service characteristics
    public const string UiLedCharacteristicUuid    = "ef680301-9b35-4933-9b10-52ffa9740042";
    public const string UiButtonCharacteristicUuid = "ef680302-9b35-4933-9b10-52ffa9740042";

    // Motion Service characteristics
    public const string MotionConfigCharacteristicUuid     = "ef680401-9b35-4933-9b10-52ffa9740042";
    public const string MotionTapCharacteristicUuid        = "ef680402-9b35-4933-9b10-52ffa9740042";
    public const string MotionOrientationCharacteristicUuid= "ef680403-9b35-4933-9b10-52ffa9740042";
    public const string MotionQuaternionCharacteristicUuid = "ef680404-9b35-4933-9b10-52ffa9740042";
    public const string MotionPedometerCharacteristicUuid  = "ef680405-9b35-4933-9b10-52ffa9740042";
    public const string MotionRawDataCharacteristicUuid    = "ef680406-9b35-4933-9b10-52ffa9740042";
    public const string MotionEulerCharacteristicUuid      = "ef680407-9b35-4933-9b10-52ffa9740042";
    public const string MotionHeadingCharacteristicUuid    = "ef680409-9b35-4933-9b10-52ffa9740042";
    public const string MotionGravityCharacteristicUuid    = "ef68040a-9b35-4933-9b10-52ffa9740042";

    // Sound Service characteristics
    public const string SoundConfigCharacteristicUuid        = "ef680501-9b35-4933-9b10-52ffa9740042";
    public const string SoundSpeakerDataCharacteristicUuid   = "ef680502-9b35-4933-9b10-52ffa9740042";
    public const string SoundSpeakerStatusCharacteristicUuid = "ef680503-9b35-4933-9b10-52ffa9740042";
    public const string SoundMicrophoneCharacteristicUuid    = "ef680504-9b35-4933-9b10-52ffa9740042";
}
