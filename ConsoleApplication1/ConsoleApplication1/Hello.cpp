#include <stdio.h>
//#include <string.h>
#include <time.h>	//���Ԍv���ɕK�v
#include <Windows.h>//���������ɕK�v
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
	//�����_�����l�z��쐬
	srand((unsigned int)time(NULL));//�����V�[�h�����ݎ����琶��
	for (int i = 0; i < vr; ++i) {
		v[i] = (int)rand();//����
	}

	int mys = GetTickCount();
	BubbleSort2(v, vr);//�o�u���\�[�g
	printf("C++��%d���o�u���\�[�g���ԁF%d�~���b\n",vr, GetTickCount() - mys);

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
//	puts("��������͂���̂ł�");
//	char str[50];
//	fgets(str, sizeof(str), stdin);
//	int num1;
//	sscanf(str, "%d", &num1);
//	return num1;
//}


