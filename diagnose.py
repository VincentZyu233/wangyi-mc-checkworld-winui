#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
WinUI 应用程序诊断工具
检查 WinUI 应用程序启动问题的各个方面
"""

import os
import sys
import subprocess
import platform
import time
from pathlib import Path

def print_header(text):
    """打印标题"""
    print(f"\n{text}")
    print("=" * 50)

def print_section(text):
    """打印章节标题"""
    print(f"\n{text}")
    print("-" * 50)

def print_success(text):
    """打印成功信息"""
    print(f"✅ {text}")

def print_error(text):
    """打印错误信息"""
    print(f"❌ {text}")

def print_warning(text):
    """打印警告信息"""
    print(f"⚠️  {text}")

def print_info(text):
    """打印信息"""
    print(f"   {text}")

def check_windows_version():
    """检查 Windows 版本"""
    print_section("1. 检查 Windows 版本...")
    
    try:
        version = platform.version()
        build_number = int(version.split('.')[-1]) if '.' in version else 0
        
        print_info(f"Windows 版本: {platform.system()} {platform.release()}")
        print_info(f"详细版本: {version}")
        print_info(f"构建号: {build_number}")
        
        if build_number < 19041:
            print_error("需要 Windows 10 19041 或更高版本")
            return False
        else:
            print_success("Windows 版本符合要求")
            return True
    except Exception as e:
        print_error(f"检查 Windows 版本失败: {e}")
        return False

def check_dotnet_runtime():
    """检查 .NET 运行时"""
    print_section("2. 检查 .NET 运行时...")
    
    try:
        result = subprocess.run(['dotnet', '--list-runtimes'], 
                          capture_output=True, text=True, encoding='utf-8')
        
        if result.returncode == 0:
            print_info("已安装的运行时:")
            runtimes = result.stdout.strip().split('\n')
            for runtime in runtimes:
                print_info(f"  {runtime}")
            
            # 检查是否有 .NET 9.0
            has_dotnet9 = any('Microsoft.NETCore.App' in r and '9.' in r for r in runtimes)
            if has_dotnet9:
                print_success(".NET 9.0 运行时已安装")
                return True
            else:
                print_warning("未找到 .NET 9.0 运行时")
                return False
        else:
            print_error(".NET 运行时未安装")
            return False
    except FileNotFoundError:
        print_error("dotnet 命令未找到，请安装 .NET SDK")
        return False
    except Exception as e:
        print_error(f"检查 .NET 运行时失败: {e}")
        return False

def check_windows_app_sdk():
    """检查 Windows App SDK"""
    print_section("3. 检查 Windows App SDK...")
    
    try:
        program_files_x86 = os.environ.get('ProgramFiles(x86)', os.environ.get('ProgramFiles', ''))
        app_sdk_path = os.path.join(program_files_x86, 'WindowsApps', 'Microsoft.WindowsAppRuntime.*')
        
        # 使用 glob 模式匹配
        import glob
        matching_dirs = glob.glob(app_sdk_path)
        
        if matching_dirs:
            print_success("Windows App SDK 已安装:")
            for dir_path in matching_dirs:
                dir_name = os.path.basename(dir_path)
                print_info(f"  {dir_name}")
            return True
        else:
            print_warning("未找到 Windows App SDK (可能使用自包含发布)")
            return False
    except Exception as e:
        print_warning(f"检查 Windows App SDK 失败: {e}")
        return False

def check_app_file():
    """检查应用程序文件"""
    print_section("4. 检查应用程序文件...")
    
    exe_path = Path(".\\WangyiMCCheckworld-v0.2.2-windows-x64.exe")
    
    if exe_path.exists():
        print_success("应用程序文件存在")
        file_size = exe_path.stat().st_size
        file_size_mb = file_size / (1024 * 1024)
        mtime = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(exe_path.stat().st_mtime))
        
        print_info(f"文件大小: {file_size_mb:.2f} MB")
        print_info(f"修改时间: {mtime}")
        print_info(f"文件路径: {exe_path.absolute()}")
        return True
    else:
        print_error("应用程序文件不存在")
        return False

def run_application():
    """尝试运行应用程序"""
    print_section("5. 尝试运行应用程序...")
    
    exe_path = Path(".\\WangyiMCCheckworld-v0.2.2-windows-x64.exe")
    
    if not exe_path.exists():
        print_info("跳过: 应用程序文件不存在")
        return False
    
    try:
        print_info("启动应用程序...")
        process = subprocess.Popen(
            [str(exe_path)],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding='utf-8',
            errors='replace'
        )
        
        # 等待进程启动
        time.sleep(2)
        
        # 检查进程是否还在运行
        poll_result = process.poll()
        
        if poll_result is None:
            print_success("应用程序正在运行")
            print_info(f"进程 ID: {process.pid}")
            
            # 等待一段时间看是否崩溃
            print_info("等待 5 秒检查应用程序状态...")
            time.sleep(5)
            
            poll_result = process.poll()
            if poll_result is None:
                print_success("应用程序仍在运行")
                # 终止进程
                process.terminate()
                try:
                    process.wait(timeout=5)
                except subprocess.TimeoutExpired:
                    process.kill()
                    process.wait()
                return True
            else:
                print_error(f"应用程序异常退出，退出代码: {poll_result}")
                return False
        else:
            print_error(f"应用程序立即退出，退出代码: {poll_result}")
            
            # 获取输出
            stdout, stderr = process.communicate(timeout=5)
            if stdout:
                print_info("标准输出:")
                for line in stdout.split('\n'):
                    if line.strip():
                        print_info(f"  {line}")
            if stderr:
                print_info("错误输出:")
                for line in stderr.split('\n'):
                    if line.strip():
                        print_info(f"  {line}")
            return False
            
    except Exception as e:
        print_error(f"运行应用程序失败: {e}")
        return False

def check_event_logs():
    """检查事件日志"""
    print_section("6. 检查事件日志...")
    
    try:
        # 使用 PowerShell 检查最近的 .NET Runtime 错误
        ps_command = '''
        Get-WinEvent -FilterHashtable @{LogName='Application'; ProviderName='.NET Runtime'} -MaxEvents 10 -ErrorAction SilentlyContinue | 
        Where-Object { $_.TimeCreated -gt (Get-Date).AddMinutes(-10) } | 
        Select-Object TimeCreated, Message | 
        Format-List
        '''
        
        result = subprocess.run(
            ['powershell', '-Command', ps_command],
            capture_output=True,
            text=True,
            encoding='utf-8',
            errors='replace'
        )
        
        if result.returncode == 0 and result.stdout.strip():
            print_error("发现最近的应用程序错误:")
            print_info(result.stdout)
            return False
        else:
            print_success("未发现最近的应用程序错误")
            return True
    except Exception as e:
        print_warning(f"检查事件日志失败: {e}")
        return False

def check_log_files():
    """检查日志文件"""
    print_section("7. 检查日志文件...")
    
    temp_dir = os.environ.get('TEMP', os.environ.get('TMP', ''))
    log_path = os.path.join(temp_dir, 'WangyiMCCheckworld_error.txt')
    
    if os.path.exists(log_path):
        print_success(f"找到日志文件: {log_path}")
        print_info("日志内容:")
        try:
            with open(log_path, 'r', encoding='utf-8', errors='replace') as f:
                for line in f:
                    print_info(f"  {line.rstrip()}")
        except Exception as e:
            print_error(f"读取日志文件失败: {e}")
        return True
    else:
        print_warning("未找到日志文件")
        return False

def main():
    """主函数"""
    print_header("WinUI 应用程序诊断工具")
    
    # 保存当前目录
    original_dir = os.getcwd()
    
    # 切换到脚本所在目录（如果是从下载目录运行）
    script_dir = Path(__file__).parent
    os.chdir(script_dir)
    
    try:
        # 运行所有检查
        results = {
            'Windows 版本': check_windows_version(),
            '.NET 运行时': check_dotnet_runtime(),
            'Windows App SDK': check_windows_app_sdk(),
            '应用程序文件': check_app_file(),
            '应用程序运行': run_application(),
            '事件日志': check_event_logs(),
            '日志文件': check_log_files(),
        }
        
        # 总结
        print_header("诊断完成")
        
        success_count = sum(1 for result in results.values() if result)
        total_count = len(results)
        
        print_info(f"检查项目: {total_count}")
        print_info(f"通过项目: {success_count}")
        print_info(f"失败项目: {total_count - success_count}")
        
        print_info("\n详细结果:")
        for check_name, result in results.items():
            status = "✅ 通过" if result else "❌ 失败"
            print_info(f"  {check_name}: {status}")
        
        print_info("\n如果问题仍然存在，请提供以上诊断信息以获得进一步帮助。")
        
    finally:
        # 恢复原始目录
        os.chdir(original_dir)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print_info("\n\n用户中断诊断")
        sys.exit(0)
    except Exception as e:
        print_error(f"诊断工具运行失败: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)