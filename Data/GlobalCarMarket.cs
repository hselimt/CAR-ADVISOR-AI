namespace CarAdvisorAPI.Data
{
    public class MarketData
    {
        public Dictionary<string, List<string>>? AvailableModels { get; set; }
        public Dictionary<string, (long Min, long Max)>? SegmentPriceRanges { get; set; }
        public string? Currency { get; set; }
    }

    public static class GlobalCarMarket
    {
        public static MarketData GetMarketData(string country)
        {
            return country switch
            {
                "Turkey" => TurkeyMarket(),
                "USA" => USAMarket(),
                "Germany" => GermanyMarket(),
                "UK" => UKMarket(),
                "Japan" => JapanMarket(),
                "UAE" => UAEMarket(),
                _ => throw new ArgumentException($"Market data not available for country: {country}")
            };
        }

        private static MarketData TurkeyMarket()
        {
            return new MarketData
            {
                Currency = "TRY",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Fiat"] = new() { "Egea", "Egea Cross", "500", "500X", "Tipo", "Doblo" },
                    ["Dacia"] = new() { "Sandero", "Sandero Stepway", "Duster", "Jogger" },
                    ["Renault"] = new() { "Clio", "Megane", "Taliant", "Austral", "Captur" },
                    ["Opel"] = new() { "Corsa", "Astra", "Crossland", "Grandland", "Mokka" },
                    ["Citroën"] = new() { "C3", "C3 Aircross", "C4", "C5 Aircross", "Berlingo" },

                    // MAINSTREAM SEGMENT
                    ["Toyota"] = new() { "Yaris", "Corolla", "Corolla Cross", "C-HR", "RAV4", "Camry", "Hilux" },
                    ["Honda"] = new() { "Civic", "CR-V", "HR-V", "Jazz" },
                    ["Hyundai"] = new() { "i10", "i20", "i20 N", "Bayon", "Kona", "Tucson", "Santa Fe", "Ioniq 5" },
                    ["Kia"] = new() { "Picanto", "Rio", "Stonic", "Ceed", "XCeed", "Proceed", "Sportage", "Sorento", "EV6" },
                    ["Nissan"] = new() { "Micra", "Juke", "Qashqai", "X-Trail" },
                    ["Mazda"] = new() { "2", "3", "CX-5", "CX-60", "MX-5" },
                    ["Ford"] = new() { "Puma", "Kuga", "Mustang Mach-E" },
                    ["Suzuki"] = new() { "Swift", "Vitara", "S-Cross", "Jimny" },

                    // UPPER-MAINSTREAM SEGMENT
                    ["Peugeot"] = new() { "208", "2008", "308", "3008", "408", "5008", "e-2008", "e-308" },
                    ["Volkswagen"] = new() { "Polo", "Taigo", "T-Cross", "T-Roc", "Tiguan", "Tiguan Allspace", "Passat", "Arteon", "ID.4", "ID.5", "ID.Buzz" },
                    ["Skoda"] = new() { "Fabia", "Scala", "Kamiq", "Karoq", "Kodiaq", "Octavia", "Superb", "Enyaq" },
                    ["SEAT"] = new() { "Ibiza", "Arona", "Leon", "Ateca", "Tarraco" },
                    ["Cupra"] = new() { "Formentor", "Leon", "Ateca" },
                    ["Volvo"] = new() { "XC40", "XC60", "XC90", "S60", "S90", "C40", "EX30" },
                    ["Subaru"] = new() { "Outback", "Forester", "XV" },

                    // PREMIUM SEGMENT
                    ["BMW"] = new() { "1 Series", "2 Series", "2 Series Gran Coupe", "3 Series", "4 Series", "5 Series", "7 Series", "8 Series", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "iX", "iX1", "iX3", "i4", "i5", "i7" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "CLA", "CLS", "GLA", "GLB", "GLC", "GLE", "GLS", "EQA", "EQB", "EQC", "EQE", "EQS" },
                    ["Audi"] = new() { "A1", "A3", "A4", "A5", "A6", "A7", "A8", "Q2", "Q3", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron", "e-tron GT" },
                    ["Lexus"] = new() { "UX", "NX", "RX", "ES", "LS" },
                    ["Alfa Romeo"] = new() { "Tonale", "Giulia", "Stelvio" },
                    ["DS"] = new() { "DS 3", "DS 4", "DS 7" },
                    ["Genesis"] = new() { "GV70", "GV80", "G70", "G80" },
                    ["Mini"] = new() { "Cooper", "Cooper S", "Countryman", "Clubman" },

                    // ULTRA-LUXURY SEGMENT
                    ["Porsche"] = new() { "718 Cayman", "718 Boxster", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Range Rover"] = new() { "Evoque", "Velar", "Sport", "Range Rover" },
                    ["Land Rover"] = new() { "Defender", "Discovery", "Discovery Sport" },
                    ["Jaguar"] = new() { "E-Pace", "F-Pace", "I-Pace", "F-Type" },
                    ["Maserati"] = new() { "Ghibli", "Levante", "Quattroporte", "Grecale" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Lamborghini"] = new() { "Urus", "Huracán", "Revuelto" },
                    ["Ferrari"] = new() { "Roma", "Portofino", "296 GTB", "SF90", "Purosangue" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model Y" },
                    ["MG"] = new() { "4", "5", "ZS EV", "MG4 Electric" },
                    ["BYD"] = new() { "Atto 3", "Seal", "Dolphin", "Han" },
                    ["Togg"] = new() { "T10X" },
                    ["Polestar"] = new() { "2", "3" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (600000, 1300000),
                    ["Mainstream"] = (1000000, 2500000),
                    ["Upper-Mainstream"] = (1500000, 3500000),
                    ["Premium"] = (2500000, 6000000),
                    ["Ultra-Luxury"] = (5000000, 30000000),
                    ["Electric"] = (1200000, 5000000)
                }
            };
        }

        private static MarketData USAMarket()
        {
            return new MarketData
            {
                Currency = "USD",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Chevrolet"] = new() { "Spark", "Trax", "Trailblazer" },
                    ["Nissan"] = new() { "Versa", "Sentra", "Kicks" },
                    ["Hyundai"] = new() { "Accent", "Venue" },
                    ["Kia"] = new() { "Rio", "Soul" },
                    ["Mitsubishi"] = new() { "Mirage" },

                    // MAINSTREAM SEGMENT
                    ["Toyota"] = new() { "Corolla", "Camry", "RAV4", "Highlander", "4Runner", "Tacoma", "Tundra", "Sienna", "Prius" },
                    ["Honda"] = new() { "Civic", "Accord", "CR-V", "Pilot", "HR-V", "Passport", "Ridgeline", "Odyssey" },
                    ["Mazda"] = new() { "Mazda3", "Mazda6", "CX-30", "CX-5", "CX-50", "CX-9", "MX-5 Miata" },
                    ["Subaru"] = new() { "Impreza", "Legacy", "Outback", "Forester", "Crosstrek", "Ascent", "WRX", "BRZ" },
                    ["Volkswagen"] = new() { "Jetta", "Passat", "Tiguan", "Atlas", "Atlas Cross Sport", "Taos", "ID.4" },
                    ["Chevrolet"] = new() { "Malibu", "Equinox", "Blazer", "Traverse", "Tahoe", "Suburban", "Silverado", "Colorado" },
                    ["Ford"] = new() { "Maverick", "Escape", "Edge", "Explorer", "Expedition", "F-150", "Ranger", "Bronco", "Bronco Sport", "Mustang" },
                    ["Jeep"] = new() { "Compass", "Cherokee", "Grand Cherokee", "Wrangler", "Gladiator", "Wagoneer", "Grand Wagoneer" },
                    ["Ram"] = new() { "1500", "2500", "3500" },
                    ["GMC"] = new() { "Terrain", "Acadia", "Yukon", "Sierra" },
                    ["Dodge"] = new() { "Hornet", "Durango", "Charger", "Challenger" },

                    // PREMIUM SEGMENT
                    ["Acura"] = new() { "Integra", "TLX", "MDX", "RDX" },
                    ["Infiniti"] = new() { "Q50", "QX50", "QX55", "QX60", "QX80" },
                    ["Lexus"] = new() { "IS", "ES", "LS", "UX", "NX", "RX", "GX", "LX", "LC" },
                    ["Genesis"] = new() { "G70", "G80", "G90", "GV60", "GV70", "GV80" },
                    ["BMW"] = new() { "2 Series", "3 Series", "4 Series", "5 Series", "7 Series", "8 Series", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "iX", "i4", "i7" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "CLA", "CLS", "GLA", "GLB", "GLC", "GLE", "GLS", "G-Class", "EQS", "EQE" },
                    ["Audi"] = new() { "A3", "A4", "A5", "A6", "A7", "A8", "Q3", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron", "e-tron GT", "RS e-tron GT" },
                    ["Cadillac"] = new() { "CT4", "CT5", "XT4", "XT5", "XT6", "Escalade", "Lyriq" },
                    ["Lincoln"] = new() { "Corsair", "Nautilus", "Aviator", "Navigator" },
                    ["Volvo"] = new() { "S60", "S90", "V60", "V90", "XC40", "XC60", "XC90", "C40" },
                    ["Alfa Romeo"] = new() { "Giulia", "Stelvio", "Tonale" },

                    // ULTRA-LUXURY SEGMENT
                    ["Porsche"] = new() { "718 Cayman", "718 Boxster", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Maserati"] = new() { "Ghibli", "Levante", "Quattroporte", "Grecale", "MC20" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Lamborghini"] = new() { "Urus", "Huracán", "Revuelto" },
                    ["Ferrari"] = new() { "Roma", "Portofino", "296 GTB", "SF90", "Purosangue", "812" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom", "Spectre" },
                    ["Aston Martin"] = new() { "Vantage", "DB12", "DBX" },
                    ["McLaren"] = new() { "Artura", "GT", "720S" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model S", "Model X", "Model Y" },
                    ["Rivian"] = new() { "R1T", "R1S" },
                    ["Lucid"] = new() { "Air" },
                    ["Polestar"] = new() { "2", "3" },
                    ["Fisker"] = new() { "Ocean" },
                    ["Chevrolet"] = new() { "Bolt EV", "Bolt EUV", "Blazer EV", "Equinox EV", "Silverado EV" },
                    ["Ford"] = new() { "Mustang Mach-E", "F-150 Lightning" },
                    ["Hyundai"] = new() { "Ioniq 5", "Ioniq 6" },
                    ["Kia"] = new() { "EV6", "EV9", "Niro EV" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (15000, 25000),
                    ["Mainstream"] = (25000, 50000),
                    ["Upper-Mainstream"] = (40000, 65000),
                    ["Premium"] = (50000, 100000),
                    ["Ultra-Luxury"] = (100000, 500000),
                    ["Electric"] = (35000, 120000)
                }
            };
        }

        private static MarketData GermanyMarket()
        {
            return new MarketData
            {
                Currency = "EUR",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Dacia"] = new() { "Sandero", "Duster", "Jogger", "Spring" },
                    ["Fiat"] = new() { "500", "Panda", "Tipo" },
                    ["Citroën"] = new() { "C3", "C3 Aircross" },
                    ["Opel"] = new() { "Corsa", "Astra", "Crossland", "Mokka" },
                    ["Renault"] = new() { "Clio", "Captur", "Arkana" },

                    // MAINSTREAM SEGMENT
                    ["Volkswagen"] = new() { "Polo", "Golf", "T-Cross", "T-Roc", "Taigo", "Tiguan", "Passat", "Arteon", "ID.3", "ID.4", "ID.5", "ID.7", "ID.Buzz" },
                    ["Skoda"] = new() { "Fabia", "Scala", "Kamiq", "Karoq", "Kodiaq", "Octavia", "Superb", "Enyaq" },
                    ["SEAT"] = new() { "Ibiza", "Arona", "Leon", "Ateca", "Tarraco" },
                    ["Cupra"] = new() { "Born", "Formentor", "Leon", "Ateca" },
                    ["Audi"] = new() { "A1", "A3", "Q2", "Q3" },
                    ["Ford"] = new() { "Fiesta", "Focus", "Puma", "Kuga", "Mustang Mach-E" },
                    ["Toyota"] = new() { "Yaris", "Corolla", "C-HR", "RAV4", "Highlander", "bZ4X" },
                    ["Hyundai"] = new() { "i10", "i20", "i30", "Bayon", "Kona", "Tucson", "Ioniq 5", "Ioniq 6" },
                    ["Kia"] = new() { "Picanto", "Rio", "Stonic", "Ceed", "XCeed", "Sportage", "Sorento", "EV6", "EV9", "Niro" },
                    ["Peugeot"] = new() { "208", "2008", "308", "3008", "408", "5008", "e-208", "e-2008" },
                    ["Nissan"] = new() { "Juke", "Qashqai", "X-Trail", "Ariya" },
                    ["Mazda"] = new() { "2", "3", "CX-5", "CX-60", "MX-5", "MX-30" },

                    // PREMIUM SEGMENT
                    ["BMW"] = new() { "1 Series", "2 Series", "3 Series", "4 Series", "5 Series", "7 Series", "8 Series", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "iX1", "iX3", "i4", "i5", "i7", "iX" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "CLA", "CLS", "GLA", "GLB", "GLC", "GLE", "GLS", "G-Class", "EQA", "EQB", "EQC", "EQE", "EQS" },
                    ["Audi"] = new() { "A4", "A5", "A6", "A7", "A8", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron", "e-tron GT" },
                    ["Volvo"] = new() { "S60", "S90", "V60", "V90", "XC40", "XC60", "XC90", "C40", "EX30", "EX90" },
                    ["Mini"] = new() { "Cooper", "Countryman", "Clubman", "Electric" },
                    ["Alfa Romeo"] = new() { "Giulia", "Stelvio", "Tonale" },

                    // ULTRA-LUXURY SEGMENT
                    ["Porsche"] = new() { "718 Cayman", "718 Boxster", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Maserati"] = new() { "Ghibli", "Levante", "Quattroporte", "Grecale", "MC20" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Lamborghini"] = new() { "Urus", "Huracán", "Revuelto" },
                    ["Ferrari"] = new() { "Roma", "Portofino", "296 GTB", "SF90", "Purosangue" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom", "Spectre" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model Y", "Model S", "Model X" },
                    ["Polestar"] = new() { "2", "3", "4" },
                    ["Smart"] = new() { "#1", "#3" },
                    ["BYD"] = new() { "Atto 3", "Seal", "Dolphin", "Han" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (12000, 25000),
                    ["Mainstream"] = (25000, 50000),
                    ["Upper-Mainstream"] = (35000, 65000),
                    ["Premium"] = (50000, 100000),
                    ["Ultra-Luxury"] = (100000, 400000),
                    ["Electric"] = (30000, 90000)
                }
            };
        }

        private static MarketData UKMarket()
        {
            return new MarketData
            {
                Currency = "GBP",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Dacia"] = new() { "Sandero", "Duster", "Jogger", "Spring" },
                    ["Fiat"] = new() { "500", "Panda", "Tipo" },
                    ["Vauxhall"] = new() { "Corsa", "Astra", "Crossland", "Mokka", "Grandland" },
                    ["Citroën"] = new() { "C3", "C3 Aircross", "C4" },
                    ["Renault"] = new() { "Clio", "Captur", "Arkana", "Austral" },

                    // MAINSTREAM SEGMENT
                    ["Ford"] = new() { "Fiesta", "Focus", "Puma", "Kuga", "Mustang", "Mustang Mach-E" },
                    ["Toyota"] = new() { "Aygo X", "Yaris", "Corolla", "C-HR", "RAV4", "Highlander", "bZ4X" },
                    ["Honda"] = new() { "Jazz", "Civic", "CR-V", "HR-V", "e:Ny1" },
                    ["Hyundai"] = new() { "i10", "i20", "i30", "Bayon", "Kona", "Tucson", "Santa Fe", "Ioniq 5", "Ioniq 6" },
                    ["Kia"] = new() { "Picanto", "Rio", "Stonic", "Ceed", "XCeed", "Sportage", "Sorento", "EV6", "EV9", "Niro" },
                    ["Nissan"] = new() { "Micra", "Juke", "Qashqai", "X-Trail", "Ariya" },
                    ["Mazda"] = new() { "2", "3", "CX-5", "CX-60", "MX-5", "MX-30" },
                    ["Peugeot"] = new() { "108", "208", "2008", "308", "3008", "5008", "e-208", "e-2008" },
                    ["Volkswagen"] = new() { "Polo", "Golf", "T-Cross", "T-Roc", "Tiguan", "ID.3", "ID.4", "ID.5", "ID.Buzz" },
                    ["Skoda"] = new() { "Fabia", "Scala", "Kamiq", "Karoq", "Kodiaq", "Octavia", "Superb", "Enyaq" },

                    // PREMIUM SEGMENT
                    ["BMW"] = new() { "1 Series", "2 Series", "3 Series", "4 Series", "5 Series", "7 Series", "X1", "X2", "X3", "X4", "X5", "X7", "iX", "i4", "i5" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "GLA", "GLB", "GLC", "GLE", "EQA", "EQB", "EQC", "EQE", "EQS" },
                    ["Audi"] = new() { "A1", "A3", "A4", "A5", "A6", "Q2", "Q3", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron" },
                    ["Volvo"] = new() { "S60", "S90", "V60", "V90", "XC40", "XC60", "XC90", "C40", "EX30", "EX90" },
                    ["Lexus"] = new() { "UX", "NX", "RX", "ES" },
                    ["Mini"] = new() { "Cooper", "Countryman", "Clubman", "Electric" },

                    // ULTRA-LUXURY SEGMENT
                    ["Range Rover"] = new() { "Evoque", "Velar", "Sport", "Range Rover" },
                    ["Land Rover"] = new() { "Defender", "Discovery", "Discovery Sport" },
                    ["Jaguar"] = new() { "E-Pace", "F-Pace", "I-Pace", "F-Type" },
                    ["Porsche"] = new() { "718", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom", "Spectre" },
                    ["Aston Martin"] = new() { "Vantage", "DB12", "DBX" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model Y", "Model S", "Model X" },
                    ["MG"] = new() { "3", "4", "5", "HS", "ZS EV", "MG4 Electric" },
                    ["Polestar"] = new() { "2", "3", "4" },
                    ["BYD"] = new() { "Atto 3", "Seal", "Dolphin" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (12000, 22000),
                    ["Mainstream"] = (20000, 40000),
                    ["Upper-Mainstream"] = (30000, 55000),
                    ["Premium"] = (45000, 90000),
                    ["Ultra-Luxury"] = (85000, 300000),
                    ["Electric"] = (25000, 80000)
                }
            };
        }

        private static MarketData JapanMarket()
        {
            return new MarketData
            {
                Currency = "JPY",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Suzuki"] = new() { "Alto", "Wagon R", "Swift", "Solio", "Hustler", "Spacia", "Jimny" },
                    ["Daihatsu"] = new() { "Mira", "Move", "Tanto", "Taft", "Rocky", "Thor" },
                    ["Nissan"] = new() { "Dayz", "Roox", "Sakura" },
                    ["Honda"] = new() { "N-BOX", "N-WGN", "N-ONE" },

                    // MAINSTREAM SEGMENT
                    ["Toyota"] = new() { "Yaris", "Aqua", "Corolla", "Crown", "Prius", "Camry", "Noah", "Voxy", "Alphard", "Vellfire", "Sienta", "C-HR", "RAV4", "Harrier", "Land Cruiser", "bZ4X" },
                    ["Honda"] = new() { "Fit", "Civic", "Accord", "Vezel", "CR-V", "Freed", "Stepwgn", "Odyssey" },
                    ["Nissan"] = new() { "Note", "Serena", "X-Trail", "Skyline", "Fuga", "Elgrand", "Ariya" },
                    ["Mazda"] = new() { "Mazda2", "Mazda3", "Mazda6", "CX-3", "CX-5", "CX-8", "CX-60", "MX-5", "MX-30" },
                    ["Subaru"] = new() { "Impreza", "Levorg", "WRX", "Forester", "Outback", "Crosstrek", "BRZ", "Solterra" },
                    ["Mitsubishi"] = new() { "eK", "Delica D:5", "Outlander", "Eclipse Cross" },

                    // PREMIUM SEGMENT
                    ["Lexus"] = new() { "UX", "NX", "RX", "LX", "ES", "IS", "LS", "LC", "RZ" },
                    ["BMW"] = new() { "1 Series", "2 Series", "3 Series", "4 Series", "5 Series", "7 Series", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "iX", "i4" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "GLA", "GLB", "GLC", "GLE", "GLS", "EQA", "EQB", "EQC", "EQE", "EQS" },
                    ["Audi"] = new() { "A1", "A3", "A4", "A5", "A6", "A7", "A8", "Q2", "Q3", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron" },
                    ["Volvo"] = new() { "XC40", "XC60", "XC90", "S60", "S90", "V60", "V90", "C40" },
                    ["Mini"] = new() { "Cooper", "Countryman", "Clubman" },

                    // ULTRA-LUXURY SEGMENT
                    ["Porsche"] = new() { "718", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Maserati"] = new() { "Ghibli", "Levante", "Quattroporte", "Grecale" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Lamborghini"] = new() { "Urus", "Huracán", "Revuelto" },
                    ["Ferrari"] = new() { "Roma", "Portofino", "296 GTB", "SF90", "Purosangue" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model Y", "Model S", "Model X" },
                    ["Nissan"] = new() { "Leaf", "Ariya" },
                    ["BYD"] = new() { "Atto 3", "Seal", "Dolphin" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (1000000, 2500000),
                    ["Mainstream"] = (2000000, 4500000),
                    ["Upper-Mainstream"] = (3500000, 6500000),
                    ["Premium"] = (5000000, 12000000),
                    ["Ultra-Luxury"] = (10000000, 50000000),
                    ["Electric"] = (3000000, 9000000)
                }
            };
        }

        private static MarketData UAEMarket()
        {
            return new MarketData
            {
                Currency = "AED",
                AvailableModels = new()
                {
                    // BUDGET SEGMENT
                    ["Nissan"] = new() { "Sunny", "Sentra" },
                    ["Hyundai"] = new() { "Accent", "Elantra" },
                    ["Kia"] = new() { "Pegas", "Cerato" },
                    ["Mitsubishi"] = new() { "Attrage", "Mirage" },

                    // MAINSTREAM SEGMENT
                    ["Toyota"] = new() { "Yaris", "Corolla", "Camry", "Avalon", "RAV4", "Fortuner", "Land Cruiser", "Prado", "Hilux" },
                    ["Honda"] = new() { "City", "Civic", "Accord", "CR-V", "Pilot", "HR-V" },
                    ["Hyundai"] = new() { "i10", "i20", "Bayon", "Kona", "Tucson", "Santa Fe", "Palisade", "Ioniq 5" },
                    ["Kia"] = new() { "Rio", "K5", "Stonic", "Sportage", "Sorento", "Telluride", "Carnival", "EV6" },
                    ["Nissan"] = new() { "Kicks", "Altima", "Maxima", "X-Trail", "Pathfinder", "Patrol", "Armada" },
                    ["Mazda"] = new() { "2", "3", "6", "CX-3", "CX-5", "CX-9", "CX-90" },
                    ["Ford"] = new() { "EcoSport", "Escape", "Edge", "Explorer", "Expedition", "F-150", "Ranger", "Bronco" },
                    ["Chevrolet"] = new() { "Spark", "Malibu", "Camaro", "Equinox", "Blazer", "Tahoe", "Suburban", "Silverado" },
                    ["GMC"] = new() { "Terrain", "Acadia", "Yukon", "Sierra" },
                    ["Jeep"] = new() { "Renegade", "Compass", "Cherokee", "Grand Cherokee", "Wrangler", "Gladiator" },

                    // PREMIUM SEGMENT
                    ["Lexus"] = new() { "ES", "IS", "LS", "UX", "NX", "RX", "GX", "LX" },
                    ["Infiniti"] = new() { "Q50", "QX50", "QX55", "QX60", "QX80" },
                    ["Genesis"] = new() { "G70", "G80", "G90", "GV60", "GV70", "GV80" },
                    ["BMW"] = new() { "2 Series", "3 Series", "4 Series", "5 Series", "7 Series", "8 Series", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "iX", "i4", "i5", "i7" },
                    ["Mercedes-Benz"] = new() { "A-Class", "C-Class", "E-Class", "S-Class", "CLA", "CLS", "GLA", "GLB", "GLC", "GLE", "GLS", "G-Class", "AMG GT", "EQB", "EQC", "EQE", "EQS" },
                    ["Audi"] = new() { "A3", "A4", "A5", "A6", "A7", "A8", "Q3", "Q4 e-tron", "Q5", "Q7", "Q8", "e-tron", "e-tron GT", "RS e-tron GT" },
                    ["Volvo"] = new() { "S60", "S90", "XC40", "XC60", "XC90", "C40" },
                    ["Land Rover"] = new() { "Discovery Sport", "Discovery", "Defender" },
                    ["Cadillac"] = new() { "CT4", "CT5", "XT4", "XT5", "XT6", "Escalade" },

                    // ULTRA-LUXURY SEGMENT
                    ["Range Rover"] = new() { "Evoque", "Velar", "Sport", "Range Rover" },
                    ["Porsche"] = new() { "718 Cayman", "718 Boxster", "911", "Taycan", "Macan", "Cayenne", "Panamera" },
                    ["Maserati"] = new() { "Ghibli", "Levante", "Quattroporte", "Grecale", "MC20" },
                    ["Bentley"] = new() { "Bentayga", "Flying Spur", "Continental GT" },
                    ["Lamborghini"] = new() { "Urus", "Huracán", "Revuelto" },
                    ["Ferrari"] = new() { "Roma", "Portofino", "296 GTB", "SF90", "Purosangue", "812" },
                    ["Rolls-Royce"] = new() { "Ghost", "Cullinan", "Phantom", "Spectre" },
                    ["Aston Martin"] = new() { "Vantage", "DB12", "DBX" },
                    ["McLaren"] = new() { "Artura", "GT", "720S" },
                    ["Bugatti"] = new() { "Chiron" },

                    // ELECTRIC SEGMENT
                    ["Tesla"] = new() { "Model 3", "Model S", "Model X", "Model Y" },
                    ["Lucid"] = new() { "Air" },
                    ["Polestar"] = new() { "2", "3" },
                    ["BYD"] = new() { "Atto 3", "Seal", "Dolphin", "Han" },
                    ["MG"] = new() { "ZS EV", "4 Electric" }
                },
                SegmentPriceRanges = new()
                {
                    ["Budget"] = (40000, 85000),
                    ["Mainstream"] = (75000, 180000),
                    ["Upper-Mainstream"] = (130000, 250000),
                    ["Premium"] = (200000, 450000),
                    ["Ultra-Luxury"] = (400000, 3000000),
                    ["Electric"] = (120000, 500000)
                }
            };
        }
    }
}