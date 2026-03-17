import subprocess
import sys
import os
import argparse


def main():
    parser = argparse.ArgumentParser(description="Build WinUI3 app")
    parser.add_argument("--proxy", "-p", help="Proxy URL")
    parser.add_argument("--cache", "-c", help="NuGet cache path")
    args = parser.parse_args()

    env = os.environ.copy()
    if args.proxy:
        env["HTTP_PROXY"] = args.proxy
        env["HTTPS_PROXY"] = args.proxy

    if args.cache:
        env["NUGET_PACKAGES"] = args.cache
        env["NUGET_HTTP_CACHE"] = os.path.join(args.cache, "http-cache")
        env["NUGET_PLUGINS_CACHE"] = os.path.join(args.cache, "plugins-cache")

    print("Building WinUI3 app...")

    if subprocess.run("dotnet restore", shell=True, env=env).returncode != 0:
        print("Restore failed!")
        return 1

    if subprocess.run("dotnet build -c Release", shell=True, env=env).returncode != 0:
        print("Build failed!")
        return 1

    if (
        subprocess.run(
            "dotnet publish -c Release -r win-x64 --self-contained true",
            shell=True,
            env=env,
        ).returncode
        != 0
    ):
        print("Publish failed!")
        return 1

    print("\nBuild completed!")
    return 0


if __name__ == "__main__":
    sys.exit(main())
