<div align="center">

# 🚀 OodleSharp

**A C# port of the Oodle compression library**  

[![GitHub release](https://img.shields.io/github/v/release/NotOfficer/OodleSharp?logo=github)](https://github.com/NotOfficer/OodleSharp/releases/latest)
[![Nuget](https://img.shields.io/nuget/v/OodleSharp?logo=nuget)](https://www.nuget.org/packages/OodleSharp)
![Nuget Downloads](https://img.shields.io/nuget/dt/OodleSharp?logo=nuget)
[![GitHub issues](https://img.shields.io/github/issues/NotOfficer/OodleSharp?logo=github)](https://github.com/NotOfficer/OodleSharp/issues)
[![License](https://img.shields.io/github/license/NotOfficer/OodleSharp)](https://github.com/NotOfficer/OodleSharp/blob/master/LICENSE)

</div>

---

## 📦 Installation

Install via [NuGet](https://www.nuget.org/packages/OodleSharp):

```powershell
Install-Package OodleSharp
```

---

## ✨ Features

- Span-based `Decompress` / `TryDecompress` methods

---

## 🔧 Example Usage

```cs
using OodleSharp;

var compressedBuffer = System.IO.File.ReadAllBytes(@"C:\Test\Example.bin");
var decompressedBuffer = new byte[decompressedSize];
var result = OodleDecompressor.Decompress(compressedBuffer, decompressedBuffer);
```

---

## 🤝 Contributing

Contributions are **welcome and appreciated**!

Whether it's fixing a typo, suggesting an improvement, or submitting a pull request — every bit helps.

---

## 📄 License

This project is licensed under the [MIT License](https://github.com/NotOfficer/OodleSharp/blob/master/LICENSE).

---

<div align="center">

⭐️ Star the repo if you find it useful!  
Feel free to open an issue if you have any questions or feedback.

</div>
