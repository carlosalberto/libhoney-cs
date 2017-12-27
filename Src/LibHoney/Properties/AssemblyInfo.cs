using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("LibHoney")]
[assembly: AssemblyDescription("LibHoney connector for .Net")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LibHoney")]
[assembly: AssemblyCopyright("Copyright © Carlos Alberto Cortez 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.9.1.0")]

#if DEBUG
[assembly: InternalsVisibleTo ("LibHoney.Tests")]
#else
[assembly: InternalsVisibleTo ("LibHoney.Tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010067ab48f95268b599e7e0c7b4cfa067950937a46b48f3559243c1d73f3a7103197448091d36ab978664fcd41fce9f1a83b0a6f139f7c46af82f8b09836048bea59ca8576f7b3138eb85763793694ec2ab7812ce9eb49f753ceb0fcb97698f117c7ee23e06fbe8b773a94880715844a03208811f49daba121d9e21f594a31a1d94")]
#endif
