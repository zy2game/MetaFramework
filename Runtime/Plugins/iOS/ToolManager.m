#import "ToolManager.h"  

@implementation ToolManager
 
void tool_init()
{	
	if ([UIApplication instancesRespondToSelector:@selector(registerUserNotificationSettings:)])
    {
        [[UIApplication sharedApplication] registerUserNotificationSettings:[UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert|UIUserNotificationTypeBadge|UIUserNotificationTypeSound categories:nil]];
    }
	[[UIApplication sharedApplication] setApplicationIconBadgeNumber:0];
}

void createNotify(void *text, int secondsFromNow) {
	UILocalNotification *newNotification = [[UILocalNotification alloc] init];
    if (newNotification) {
        newNotification.timeZone = [NSTimeZone defaultTimeZone];
		newNotification.repeatInterval = 0;
        newNotification.fireDate = [NSDate dateWithTimeIntervalSinceNow:secondsFromNow];
        newNotification.alertBody = [NSString stringWithUTF8String:text];
		newNotification.alertAction = @"打开";
        newNotification.applicationIconBadgeNumber = 1;
        newNotification.soundName = UILocalNotificationDefaultSoundName;
        [[UIApplication sharedApplication] scheduleLocalNotification:newNotification];
    }
    NSLog(@"Post new localNotification:%@", newNotification);
    [newNotification release];
}

void cancelNotify() {
	[[UIApplication sharedApplication] cancelAllLocalNotifications];
}

@end

@implementation PhotoManager  

- (UIImage *) captureScreen {
    UIWindow *keyWindow = [[UIApplication sharedApplication] keyWindow];
    CGRect rect = [keyWindow bounds];
    UIGraphicsBeginImageContext(rect.size);
    CGContextRef context = UIGraphicsGetCurrentContext();
    [keyWindow.layer renderInContext:context];   
    UIImage *img = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return img;
}

- ( void ) imageSaved: ( UIImage *) image didFinishSavingWithError:( NSError *)error   
    contextInfo: ( void *) contextInfo  
{  
    NSLog(@"保存结束");  
    if (error != nil) {  
        NSLog(@"有错误");  
    }  
}  

void _save_photo(char *readAddr)  
{  
    NSString *strReadAddr = [NSString stringWithUTF8String:readAddr];  
    UIImage *img = [UIImage imageWithContentsOfFile:strReadAddr];  
    NSLog(@"%@",[NSString stringWithFormat:@"w:%f, h:%f", img.size.width, img.size.height]);
    NSLog(@"%@",[NSString stringWithFormat:@"%s",readAddr ]);
    PhotoManager *instance = [PhotoManager alloc];  
    UIImageWriteToSavedPhotosAlbum(img, instance,   
        @selector(imageSaved:didFinishSavingWithError:contextInfo:), nil);  
}  
@end

@implementation RRYKeyChain

static NSString * const kRHDictionaryKey = @"com.yymoon.snowball.dictionaryKey";
static NSString * const kRHKeyChainKey = @"com.yymoon.snowball.keychainKey";

- (NSMutableDictionary *)getKeychainQuery:(NSString *)service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:
            (id)kSecClassGenericPassword,(id)kSecClass,
            service, (id)kSecAttrService,
            service, (id)kSecAttrAccount,
            (id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible,
            nil];
}

- (void)save:(NSString *)service data:(id)data {
    //Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    //Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data] forKey:(id)kSecValueData];
    //Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}

- (id)load:(NSString *)service {
    id ret = nil;
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Configure the search setting
    //Since in our simple case we are expecting only a single attribute to be returned (the password) we can set the attribute kSecReturnData to kCFBooleanTrue
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        @try {
            ret = [NSKeyedUnarchiver unarchiveObjectWithData:(NSData *)keyData];
        } @catch (NSException *e) {
            NSLog(@"Unarchive of %@ failed: %@", service, e);
        } @finally {
        }
    }
    if (keyData)
        CFRelease(keyData);
    return ret;
}

- (void)rhKeyChainSave:(NSString *)service {
    NSMutableDictionary *tempDic = [NSMutableDictionary dictionary];
    [tempDic setObject:service forKey:kRHDictionaryKey];
    [self save:kRHKeyChainKey data:tempDic];
}

- (NSString *)rhKeyChainLoad{
    NSMutableDictionary *tempDic = (NSMutableDictionary *)[self load:kRHKeyChainKey];
    return [tempDic objectForKey:kRHDictionaryKey];
}

@end

static RRYKeyChain *kc;
void KeyChainSave(const char *textList)
{
	if(kc == NULL)
	{
		kc = [[RRYKeyChain alloc] init];
	}
	NSString *text = [NSString stringWithUTF8String: textList];
	[kc rhKeyChainSave: text];
}

void KeyChainLoad()
{
	if(kc == NULL)
	{
		kc = [[RRYKeyChain alloc] init];
	}
	NSString *s = [kc rhKeyChainLoad];
	if (s == nil)
	{
		UnitySendMessage("Tool", "load_kc_callback", "");
	}
	else
	{
		UnitySendMessage("Tool", "load_kc_callback", [s UTF8String]);
	}
}
