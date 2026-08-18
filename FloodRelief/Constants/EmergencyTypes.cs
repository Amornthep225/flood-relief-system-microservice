namespace FloodRelief.Constants
{
    public static class EmergencyTypes
    {
        public const string Evacuation = "Evacuation";
        public const string Trapped = "Trapped";
        public const string Injured = "Injured";
        public const string Medical = "Medical";
        public const string RoofTrapped = "RoofTrapped";
        public const string RapidFlood = "RapidFlood";
        public const string Other = "Other";

        public static readonly string[] All =
        {
            Evacuation,
            Trapped,
            Injured,
            Medical,
            RoofTrapped,
            RapidFlood,
            Other
        };

        public static readonly object[] Options =
        {
            new
            {
                value = Evacuation,
                label = "ต้องการอพยพ",
                description = "ไม่สามารถอยู่ในพื้นที่ได้อย่างปลอดภัย",
                icon = "directions_run"
            },

            new
            {
                value = Trapped,
                label = "ติดอยู่ในพื้นที่น้ำท่วม",
                description = "ออกจากพื้นที่ด้วยตนเองไม่ได้",
                icon = "flood"
            },

            new
            {
                value = Injured,
                label = "มีผู้บาดเจ็บ",
                description = "มีผู้บาดเจ็บและต้องการเจ้าหน้าที่เข้าช่วยเหลือ",
                icon = "personal_injury"
            },

            new
            {
                value = Medical,
                label = "ผู้ป่วยฉุกเฉิน",
                description = "มีผู้ป่วยที่ต้องการความช่วยเหลือเร่งด่วน",
                icon = "medical_services"
            },

            new
            {
                value = RoofTrapped,
                label = "ติดอยู่บนอาคาร/หลังคา",
                description = "ระดับน้ำสูงและไม่สามารถลงจากอาคารได้",
                icon = "roofing"
            },

            new
            {
                value = RapidFlood,
                label = "น้ำเพิ่มระดับอย่างรวดเร็ว",
                description = "สถานการณ์มีแนวโน้มอันตรายอย่างรวดเร็ว",
                icon = "warning"
            },

            new
            {
                value = Other,
                label = "เหตุฉุกเฉินอื่น ๆ",
                description = "เหตุฉุกเฉินที่ไม่อยู่ในรายการข้างต้น",
                icon = "sos"
            }
        };
    }
}