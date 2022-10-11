#import <Foundation/Foundation.h>  
#import <Security/Security.h>

@interface ToolManager : NSObject  
@end
   
@interface PhotoManager : NSObject  
- ( void ) imageSaved: ( UIImage *) image didFinishSavingWithError:( NSError *)error   
    contextInfo: ( void *) contextInfo;  
@end

@interface RRYKeyChain : NSObject
- (void)save:(NSString *)service data:(id)data;
- (id)load:(NSString *)service;
- (void)rhKeyChainSave:(NSString *)service;
- (NSString *)rhKeyChainLoad;
@end
