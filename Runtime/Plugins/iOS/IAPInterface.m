//
//  UJSInterface.m
//  Unity-iPhone
//
//  Created by MacMini on 14-5-15.
//
//

#import "IAPInterface.h"
#import "IAPManager.h"
#import <sys/utsname.h>

@implementation IAPInterface

IAPManager *iapManager = nil;

void InitIAPManager(){
    iapManager = [[IAPManager alloc] init];
    [iapManager attachObserver];
}

void FiniIAPManager(){
    [iapManager dettachObserver];
}

bool IsProductAvailable(){
    return [iapManager CanMakePayment];
}

void RequstProductInfo(void *p){
    NSString *list = [NSString stringWithUTF8String:p];
    NSLog(@"productKey:%@",list);
    [iapManager requestProductData:list];
}

void BuyProduct(void *p){
    [iapManager buyRequest:[NSString stringWithUTF8String:p]];
}
@end
