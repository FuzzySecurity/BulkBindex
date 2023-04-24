![CI](https://github.com/FuzzySecurity/BulkBindex/actions/workflows/auto-update.yml/badge.svg)

# BulkBindex

`BulkBindex` is a **very** DIY extension for [`Winbindex`](https://winbindex.m417z.com/) by [@m417z](https://twitter.com/m417z). All the great work is done there really!

`BulkBindex` fetches compressed JSON files from the `Winbindex` repository and processes those to download all binaries related to a specific Windows build and month. It can just be really useful to have everything collected after `patch tuesday` for example.

## Usage

`BulkBindex` is built on .NET6 so you can compile and run it anywhere. Command line usage is shown below.

```
// Windows
BulkBindex.exe -b 11-22H2 -d 2023-04

// Nix
dotnet BulkBindex.dll -b 11-22H2 -d 2023-04
```

#### Where do I get build names?

`BulkBindex` uses the same build identifiers as on `Winbindex`. For example, in the listing for `ntdll` below you can see the version is `11-22H2`. Note however that sometimes, other applicable versions are specified in `otherWindowsVersions` (e.g., `21H2` etc.).

```json
{
    "fileInfo": {
        "description": "NT Layer DLL",
        "machineType": 34404,
        "md5": "c9b7eb6b6320deb5a9dc6ab23be3029d",
        "sha1": "ac9d441181e3b35d4d321002ca5b4d26aa59615d",
        "sha256": "abb30adf05bd71ae8283d8a44e55268de3c408421ec059c626b92b9168adc0f9",
        "signatureType": "Overlay",
        "signingDate": [
            "2023-03-18T02:15:00"
        ],
        "signingStatus": "Signed",
        "size": 2174872,
        "timestamp": 3085964618,
        "version": "10.0.22621.1485 (WinBuild.160101.0800)",
        "virtualSize": 2179072
    },
    "windowsVersions": {
        "11-22H2": {
            "KB5023778": {
                "assemblies": {
                    "amd64_microsoft-windows-ntdll_31bf3856ad364e35_10.0.22621.1485_none_38c42af777bdcc16": {
                        "assemblyIdentity": {
                            "buildType": "release",
                            "language": "neutral",
                            "name": "Microsoft-Windows-Ntdll",
                            "processorArchitecture": "amd64",
                            "publicKeyToken": "31bf3856ad364e35",
                            "version": "10.0.22621.1485",
                            "versionScope": "nonSxS"
                        },
                        "attributes": [
                            {
                                "destinationPath": "$(runtime.system32)\\",
                                "importPath": "$(build.nttree)\\",
                                "name": "ntdll.dll",
                                "sourceName": "ntdll.dll",
                                "sourcePath": ".\\"
                            }
                        ]
                    }
                },
                "updateInfo": {
                    "heading": "March 28, 2023&#x2014;KB5023778 (OS Build 22621.1485) Preview",
                    "releaseDate": "2023-03-28",
                    "releaseVersion": "22621.1485",
                    "updateUrl": "https://support.microsoft.com/help/5023778"
                }
            },
            "KB5025239": {
                "assemblies": {
                    "amd64_microsoft-windows-ntdll_31bf3856ad364e35_10.0.22621.1485_none_38c42af777bdcc16": {
                        "assemblyIdentity": {
                            "buildType": "release",
                            "language": "neutral",
                            "name": "Microsoft-Windows-Ntdll",
                            "processorArchitecture": "amd64",
                            "publicKeyToken": "31bf3856ad364e35",
                            "version": "10.0.22621.1485",
                            "versionScope": "nonSxS"
                        },
                        "attributes": [
                            {
                                "destinationPath": "$(runtime.system32)\\",
                                "importPath": "$(build.nttree)\\",
                                "name": "ntdll.dll",
                                "sourceName": "ntdll.dll",
                                "sourcePath": ".\\"
                            }
                        ]
                    }
                },
                "updateInfo": {
                    "heading": "April 11, 2023&#x2014;KB5025239 (OS Build 22621.1555)",
                    "releaseDate": "2023-04-11",
                    "releaseVersion": "22621.1555",
                    "updateUrl": "https://support.microsoft.com/help/5025239"
                }
            }
        }
    }
}
```

## Downloads & Automation

When you run `BulkBindex` you should expect about a 20 minute runtime to fetch all binaries for a single build (depending on bandwidth). In this repository I have also added `GitHub Workflow` automation which allows workers to fetch builds for various versions and add them to the same release.

Further testing is needed but a release has already been uploaded for `April 2023 -> 11 22H2 & 10 22H2`. The intention is to add a schedule to the workflow so these releases are created right after `patch tuesday` starting from the next cycle.

## Halp?!

The code is still quite DIY. I am more than happy to receive PR's both on `BulkBindex` and the `GitHub Workflow` automation! Additions which also fetch `PDB's` welcome!