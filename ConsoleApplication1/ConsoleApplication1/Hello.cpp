#include <stdio.h>
//#include <string.h>
#include <time.h>	//時間計測に必要
#include <Windows.h>//乱数生成に必要
//#include <limits.h>
//#include <float.h>
//#include "utility.h"


//int GetInputInteger();
void BubbleSort();
void BubbleSort2(int v[], int vr);


int main(void)
{
	
	const int vr = 10000;
	int v[vr];
	//ランダム数値配列作成
	srand((unsigned int)time(NULL));//乱数シードを現在時から生成
	for (int i = 0; i < vr; ++i) {
		v[i] = (int)rand();//乱数
	}

	int mys = GetTickCount();
	BubbleSort2(v, vr);//バブルソート
	printf("C++で%d件バブルソート時間：%dミリ秒\n",vr, GetTickCount() - mys);

	return 0;
}
void BubbleSort(void)
{
	const int vr = 10000;
	int hai[vr];
	int i; int j;
	for (i = 0; i < vr; ++i) {
		hai[i] = vr - i;
	}
	int tmp;
	//time_t st = time(NULL);
	int mys = GetTickCount();
	for (i = 0; i < vr; ++i) {
		for (j = 0; j < vr - 1; ++j) {
			if (hai[j] > hai[j + 1]) {
				tmp = hai[j];
				hai[j] = hai[j + 1];
				hai[j + 1] = tmp;
			}
		}
	}

	//time_t t = time(NULL);
	//printf("%lf\n", (double)t);
	//printf("%lf\n", (long)t - (long)st);

	printf("%d\n", GetTickCount() - mys);

}

void BubbleSort2(int v[],int vr)
{
	int tmp;
	for (int i = 0; i < vr; ++i) {
		for (int j = 0; j < vr - 1; ++j) {
			if (v[j] > v[j + 1]) {
				tmp = v[j];
				v[j] = v[j + 1];
				v[j + 1] = tmp;
			}
		}
	}
}
//
//void BubbleSort3(int v[], int vr)
//{
//	int tmp;
//	for (int i = 0; i < vr; ++i) {
//		for (int j = 0; j < vr - 1; ++j) {
//			if (v[j] > v[j + 1]) {
//				tmp = v[j];
//				v[j] = v[j + 1];
//				v[j + 1] = tmp;
//			}
//		}
//	}
//}



//int GetInputInteger()
//{
//	puts("整数を入力するのです");
//	char str[50];
//	fgets(str, sizeof(str), stdin);
//	int num1;
//	sscanf(str, "%d", &num1);
//	return num1;
//}


