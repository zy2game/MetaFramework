#import <Foundation/Foundation.h>

@ interface Clipboard : NSObject

extern "C"
{
	/*  compare the namelist with system processes  */
	void copyTextToClipboard(const char *textList);
}

@end
