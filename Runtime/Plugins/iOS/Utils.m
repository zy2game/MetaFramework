//
//  Utils.m
//  Unity-iPhone
//
//  Created by WENJIE HE on 2017/12/28.
//

#import <Foundation/Foundation.h>
#import <sys/utsname.h>

@implementation Utils

+(NSString*)getdevice
{
    struct utsname systemInfo;
    uname(&systemInfo);
    NSString* deviceInfo = [NSString stringWithCString:systemInfo.machine encoding:NSUTF8StringEncoding];
    return deviceInfo;
}

+(BOOL)IsIphoneX
{
    NSString* deviceInfo = [Utils getdevice];
    if([deviceInfo isEqualToString:@"iPhone10,3"] || [deviceInfo isEqualToString:@"iPhone10,6"])
        return TRUE;
    else
        return FALSE;
}

@end
