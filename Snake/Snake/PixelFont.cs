using System.Collections.Generic;

namespace Snake
{
    class PixelFont
    {
        int width = 5;
        int height = 5;

        Dictionary<char, byte[]> characters = new Dictionary<char, byte[]>();

        public PixelFont()
        {
            characters = new Dictionary<char, byte[]>()
            {
                ['0'] = new byte[]
                {
                    0b01110,
                    0b10001,
                    0b10001,
                    0b10001,
                    0b01110,
                },
                ['1'] = new byte[]
                {
                    0b01100,
                    0b10100,
                    0b00100,
                    0b00100,
                    0b11111,
                },
                ['2'] = new byte[]
                {
                    0b11110,
                    0b00001,
                    0b01111,
                    0b10000,
                    0b11111,
                },
                ['3'] = new byte[]
                {
                    0b11110,
                    0b00001,
                    0b01110,
                    0b00001,
                    0b11110,
                },
                ['4'] = new byte[]
                {
                    0b00110,
                    0b01010,
                    0b10010,
                    0b11111,
                    0b00010,
                },
                ['5'] = new byte[]
                {
                    0b11111,
                    0b10000,
                    0b11110,
                    0b00001,
                    0b11110,
                },
                ['6'] = new byte[]
                {
                    0b01111,
                    0b10000,
                    0b11110,
                    0b10001,
                    0b01110,
                },
                ['7'] = new byte[]
                {
                    0b11111,
                    0b10001,
                    0b00010,
                    0b00100,
                    0b00100,
                },
                ['8'] = new byte[]
                {
                    0b01110,
                    0b10001,
                    0b01110,
                    0b10001,
                    0b01110,
                },
                ['9'] = new byte[]
                {
                    0b01110,
                    0b10001,
                    0b01111,
                    0b00001,
                    0b11110,
                }
            };
        }
    }
}
