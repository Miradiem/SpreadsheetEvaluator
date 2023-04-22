using FluentAssertions;
using SpreadsheetEvaluator.Api;
using SpreadsheetEvaluator.App;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SpreadsheetEvaluator.Tests.App
{
    public class EvaluationTests
    {
        [Fact]
        public void ShouldEvaluateCorrectSheetData()
        {
            var sut = CreateSut();
            var data = CorrectTestData();
            var expectedData = CorrectEvaluatedTestData();

            var result = data.Select(x => sut.EvaluateSpreadsheet(x));
            var expectedResult = expectedData.Select(x => x.Data);

            foreach (var expectedObject in expectedResult)
            {
                result.Should().ContainEquivalentOf(expectedObject);
            }
        }

        [Fact]
        public void ShouldEvaluateIncorrectSheetData()
        {
            var sut = CreateSut();
            var data = IncorrectTestData();
            var expectedData = IncorrectEvaluatedTestData();

            var result = data.Select(x => sut.EvaluateSpreadsheet(x));
            var expectedResult = expectedData.Select(y => y.Data);

            foreach (var expectedObject in expectedResult)
            {
                result.Should().ContainEquivalentOf(expectedObject);
            }
        }

        private List<SheetData> CorrectTestData() =>
            new List<SheetData>()
            { 
                new SheetData()
                {
                    Id = "sheet-0",
                    Data = new object[][]
                    {
                    }
                },
                new SheetData()
                {
                    Id = "sheet-1",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            2,
                            4,
                            8,
                            16
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-2",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            5,
                            "=A1",
                            15,
                            "=C1"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-3",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            12,
                            13,
                            "=SUM(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-4",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "=MULTIPLY(B1, C1)",
                            10,
                            20
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-5",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            6,
                            4,
                            "=DIVIDE(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-6",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            3,
                            1,
                            "=GT(A1, B1)"
                        },
                        new object[]
                        {
                            1,
                            3,
                            "=GT(A2, B2)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-7",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10.75,
                            10.75,
                            "=EQ(A1, B1)"
                        },
                        new object[]
                        {
                            10.74,
                            10.75,
                            "=EQ(A2, B2)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-8",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            "=NOT(A2)"
                        },
                        new object[]
                        {
                            false,
                            "=NOT(A1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-9",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            true,
                            true,
                            "=AND(A1, B1, C1)"
                        },
                        new object[]
                        {
                            true,
                            true,
                            false,
                            "=AND(A2, B2, C2)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-10",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            false,
                            "=OR(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-11",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            20,
                            "=IF(GT(A1, B1), A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-12",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "=CONCAT(\"Hello\", \", \", \"World!\")"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-13",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "=CONCAT(\"Hello\", \", \", \"CONCAT(\"Hello\", \", \", \"CONCAT(\"Hello\", \", \", \"World!\")\")\")"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-14",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            40,
                            "=DIVIDE(MULTIPLY(A1, B1), SUM(A1, B1))"
                        }
                    }
                }
            };

        private List<SheetData> CorrectEvaluatedTestData() =>
            new List<SheetData>()
            {
                new SheetData()
                {
                    Id = "sheet-0",
                    Data = new object[][]
                    {
                    }
                },
                new SheetData()
                {
                    Id = "sheet-1",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            2,
                            4,
                            8,
                            16
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-2",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            5,
                            5,
                            15,
                            15
                        }
                    }
                },
                new SheetData()
                    {
                        Id = "sheet-3",
                        Data = new object[][]
                        {
                            new object[]
                            {
                                12,
                                13,
                                25
                            }
                        }
                    },
                new SheetData()
                    {
                        Id = "sheet-4",
                        Data = new object[][]
                        {
                            new object[]
                            {
                                200,
                                10,
                                20
                            }
                        }
                    },
                new SheetData()
                {
                    Id = "sheet-5",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            6,
                            4,
                            1.5
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-6",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            3,
                            1,
                            true
                        },
                        new object[]
                        {
                            1,
                            3,
                            false
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-7",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10.75,
                            10.75,
                            true
                        },
                        new object[]
                        {
                            10.74,
                            10.75,
                            false
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-8",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            true
                        },
                        new object[]
                        {
                            false,
                            false
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-9",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            true,
                            true,
                            true
                        },
                        new object[]
                        {
                            true,
                            true,
                            false,
                            false
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-10",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            true,
                            false,
                            true
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-11",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            20,
                            20
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-12",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-13",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, Hello, Hello, World!"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-14",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            40,
                            8
                        }
                    }
                } 
            };

        private List<SheetData> IncorrectTestData() =>
            new List<SheetData>()
            {
                new SheetData()
                {
                    Id = "sheet-15",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            5,
                            "=A1",
                            15,
                            "=C2"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-16",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            12,
                            "Hello, World!",
                            "=SUM(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-17",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "=MULTIPLY(B1, C1)",
                            "Hello, World!",
                            20
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-18",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            4,
                            "=DIVIDE(A1, B1)"
                        },
                        new object[]
                        {
                            6,
                            "Hello, World!",
                            "=DIVIDE(A2, B2)"
                        },
                        new object[]
                        {
                            6,
                            0,
                            "=DIVIDE(A3, B3)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-19",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            1,
                            "=GT(A1, B1)"
                        },
                        new object[]
                        {
                            1,
                            "Hello, World!",
                            "=GT(A2, B2)"
                        },
                        new object[]
                        {
                            1,
                            2,
                            "=GT(A3, B3, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-20",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10.75,
                            "Hello, World!",
                            "=EQ(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-21",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            "=NOT(A1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-22",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            true,
                            true,
                            "=AND(A1, B1, C1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-23",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            false,
                            "=OR(A1, B1)"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-24",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            20,
                            "=IF(GT(A1, B1), A1)"
                        },
                        new object[]
                        {
                            "Hello, World!",
                            "Goodbye, World!",
                            "=IF(A2 > B2, A2, B2)"
                        }
                    }
                }
            };

        private List<SheetData> IncorrectEvaluatedTestData() =>
            new List<SheetData>()
            {
                new SheetData()
                {
                    Id = "sheet-15",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            5,
                            5,
                            15,
                            $"#ERROR: C2 cell does not exist."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-16",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            12,
                            "Hello, World!",
                            "#ERROR: =SUM incompatible type."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-17",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "#ERROR: =MULTIPLY incompatible type.",
                            "Hello, World!",
                            20
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-18",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            4,
                            "#ERROR: =DIVIDE incompatible numerator."
                        },
                        new object[]
                        {
                            6,
                            "Hello, World!",
                            "#ERROR: =DIVIDE incompatible denominator."
                        },
                        new object[]
                        {
                            6,
                            0,
                            "#ERROR: =DIVIDE cannot divide by zero."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-19",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            1,
                            "#ERROR: =GT incompatible type (first)."
                        },
                        new object[]
                        {
                            1,
                            "Hello, World!",
                            "#ERROR: =GT incompatible type (second)."
                        },
                         new object[]
                        {
                            1,
                            2,
                            "#ERROR: =GT requires 2 parameters."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-20",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10.75,
                            "Hello, World!",
                            "#ERROR: =EQ incompatible types."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-21",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            "#ERROR: =NOT incompatible type."
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-22",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            true,
                            true,
                            "#ERROR: =AND incompatible type"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-23",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            "Hello, World!",
                            false,
                            "#ERROR: =OR incompatible type"
                        }
                    }
                },
                new SheetData()
                {
                    Id = "sheet-24",
                    Data = new object[][]
                    {
                        new object[]
                        {
                            10,
                            20,
                            "#ERROR: =IF requires 3 parameters."
                        },
                        new object[]
                        {
                            "Hello, World!",
                            "Goodbye, World!",
                            "#ERROR: =IF incompatible type."
                        }
                    }
                }
            };

        private Evaluation CreateSut() =>
            new Evaluation();
    }
}