2021-2022ѧ���2ѧ��

# ʵ�鱨��



![](../../../../AAA%E5%A4%A7%E5%AD%A6%E6%96%87%E4%BB%B6/2022%E7%BC%96%E8%AF%91%E5%8E%9F%E7%90%86/%E5%A4%A7%E4%BD%9C%E4%B8%9A%E5%8F%82%E8%80%83/FinalOfCompile-master/FinalOfCompile-master/img/zucc.png)

- �γ�����: <u>�������ԭ�������</u>

- ʵ����Ŀ: <u>��ĩ����ҵ</u>

- רҵ�༶: <u>����1902</u>

- ѧ��ѧ��: <u>31901048</u>

- ѧ������: <u>�����</u>

- ʵ��ָ����ʦ: <u>��ܿ</u>

  

## 1.��Ŀ���

- ����ԭ��γ��Ǽ�������רҵѧ����[���޿γ�](https://baike.baidu.com/item/%E5%BF%85%E4%BF%AE%E8%AF%BE%E7%A8%8B/1257453)�͸ߵ�ѧУ���������רҵ�˲ŵĻ��������Ŀγ̣�ͬʱҲ�Ǽ����רҵ�γ������Ѽ�����սѧϰ�����Ŀγ�֮һ�� ��ּ�ڽ���[�������](https://baike.baidu.com/item/%E7%BC%96%E8%AF%91%E7%A8%8B%E5%BA%8F/8290180)�����һ��ԭ��ͻ������������ݰ������Ժ��ķ���[�ʷ�����](https://baike.baidu.com/item/%E8%AF%8D%E6%B3%95%E5%88%86%E6%9E%90/8853461)���﷨������[�﷨�Ƶ�����](https://baike.baidu.com/item/%E8%AF%AD%E6%B3%95%E5%88%B6%E5%AF%BC%E7%BF%BB%E8%AF%91/2665709)���м�������ɡ�[�洢����](https://baike.baidu.com/item/%E5%AD%98%E5%82%A8%E7%AE%A1%E7%90%86/9827115)�������Ż���Ŀ��������ɡ� 

- ���α���ԭ�����ҵ�����ǻ�����ʦ����ĩ����ҵ�ļ����������microc���ο�һЩ��������������ɵġ�
- ������ҵ������һ������ɵģ������Ƚ������˶��飬����ʵ�ֵĹ��ܱȽ���һЩ������һЩ���ܵ�ʵ��Ҳ��̫���룬�������뷨ȴ�޷�ʵ�֡���ˣ�����ԭ���кܶ���Ҫ�Ҽ���ѧϰ�ĵط���



## 2.�ļ�˵��

### interpreter  ������

```
AbstractSyn.fs         	 �����﷨
lex.fsl				     fslex�ʷ�����
par.fsy					 fsyacc�﷨����
Parse.fs				 �﷨������
Interp.fs				 ������
test/ex1.c-ex12.c 		 ���ӳ���
interpc.fsproj			 ��Ŀ�ļ�
```



## 3.�÷�

### ǰ��

#### ������

- д��AbstractSyn.fs

- Par.fsy����AbstractSyn��ͨ��Par.fsy����Par.fs�﷨������

  ```sh
  dotnet "C:\Users\Administrator\.nuget\packages\fslexyacc\10.2.0\build\/fsyacc/netcoreapp3.1\fsyacc.dll"  -o "Par.fs" --module Par Par.fsy
  ```

  ![1654604895297](assets/1654604895297.png)

  

- Lex.fsl �����д� Par��ͨ��Lex.fsl����Lex.fs�ʷ�������

  ```sh
  dotnet "C:\Users\Administrator\.nuget\packages\fslexyacc\10.2.0\build\/fslex/netcoreapp3.1\fslex.dll"  -o "Lex.fs" --module Lex --unicode Lex.fsl
  ```

  ![1654604910431](assets/1654604910431.png)

  

- ���������г����������ð�

  ```sh
  dotnet fsi
  #r "nuget: FsLexYacc";;  //��Ӱ�����
  ```

- �������иñ�����

  ```sh
  #load "AbstractSyn.fs" "Par.fs" "Lex.fs" "Debug.fs" "Parse.fs" "machine.fs" "Contcomp.fs" "ParseAndcontComp.fs" ;;
  ```

- open parseAndComp��֮�����fsi�н��б��� *.c�ļ�,�����м��ʾ *.out

  ```sh
  open ParseAndContcomp;;
  contCompileToFile (fromFile "test/switch.c") "test/switch.out";;  
  ```

- �����м��ʾ

  ```sh
  24 19 0 5 25 15 1 15 1 13 0 0 12 15 -1 13 0 1 1 0 1 12 15 -1 13 0 1 1 11 0 1 6 17 60 13 13 0 1 1 11 13 0 1 1 11 1 12 15 -1 13 13 11 0 5 1 12 15 -1 16 70 13 0 1 1 11 0 3 6 17 81 13 13 11 0 1 1 12 15 -1 16 109 13 0 1 1 11 0 5 6 17 109 13 13 11 13 0 1 1 11 13 0 1 1 11 3 1 12 15 -1 13 11 22 21 2
  ```

  ![1654605488274](assets/1654605488274.png)

- ���⹹����ʽ

  ```
  # ���� microc.exe ���������� 
  dotnet restore  microc.fsproj # ��ѡ
  dotnet clean  microc.fsproj   # ��ѡ
  dotnet build  microc.fsproj   # ���� ./bin/Debug/net6.0/microc.exe
  
  dotnet run --project microcc.fsproj example/ex1.c    # ִ�б����������� ex1.c�������  ex1.out �ļ�
  dotnet run --project microcc.fsproj -g test/switch.c   # -g �鿴������Ϣ
  ./bin/Debug/net5.0/microcc.exe test/switch.c  # ֱ��ִ�й�����.exe�ļ���ͬ��Ч��
  ```

  

#### ������

- ����ɨ������ͬ�ϣ�

  ```sh
  dotnet "C:\Users\Administrator\.nuget\packages\fslexyacc\10.2.0\build\/fslex/netcoreapp3.1\fslex.dll"  -o "Lex.fs" --module Lex --unicode Lex.fsl
  ```

- ���ɷ�����(ͬ��)

  ```sh
  dotnet "C:\Users\gm\.nuget\packages\fslexyacc\10.2.0\build\/fsyacc/netcoreapp3.1\fsyacc.dll"  -o "Par.fs" --module Par Par.fsy
  ```

- ���������г����������ð�

  ```sh
  # ���������г���
  dotnet fsi 
  #r "nuget: FsLexYacc";;  //��Ӱ�����
  ```

- �������

  ```sh
  #load "AbstractSyn.fs" "Debug.fs" "Par.fs" "Lex.fs" "Parse.fs" "interp.fs" "ParseAndRun.fs" ;; 
  ```

- ����ģ�鲢����

  ```sh
  open ParseAndRun;;    //����ģ�� ParseAndRun
  fromFile "test\switch.c";;    //��ʾ ex1.c���﷨��
  run (fromFile "test\switch.c") [];; //����ִ�� switch.c
  ```

  

![1654606357503](assets/1654606357503.png)



### ���

- javac����������� *.java �ļ�

  ```sh
  javac Machine.java  //���������
  ```

- ������������б������ɵ�.out�ļ�

  ```sh
  java Machine switch.out
  ```

- java Machinetrace ׷�ٶ�ջ�仯

  ```sh
  java Machinetrace switch.out
  ```



## 4.����ʵ��

### switch-case����

- ˵����Switch��һЩ������������Ǳ����֣������ô��������ǽ���[�ж�](https://baike.baidu.com/item/%E5%88%A4%E6%96%AD/33345)ѡ����[C����](https://baike.baidu.com/item/C%E8%AF%AD%E8%A8%80/105958)��˵��switch��������䣩����case break defaultһ��ʹ�á� ���������ʽ��������������һ��case����еĳ������ʱ����ִ�д�case���������䣬��������ȥִ�к�������case����е���䣬��������break;�������switch���Ϊֹ������������ʽ����������case���ĳ��������������ִ��default����е���䡣 

- ��������

  ```c
  int main(){
      int i;
      int n;
      i=0;
      n=1;
      switch(n){
          case 1:{i=n+n;i=i+5;}
          case 2:{i=i*2;}
          case 3:{i=i+1;break;}
          case 5:i=i+n*n;
      }
      print i;
  }
  ```

- ����Ϊָ�

  ![1654612914848](assets/1654612914848.png)

  ![1654612925357](assets/1654612925357.png)

- ���н��

  ![1654612944381](assets/1654612944381.png)

- ����ջ׷��

  ![1654613005760](assets/1654613005760.png)

  

### break����

- ˵������һ��ѭ���У�������Ҫbreak�������˳�ѭ���������һֱ������ȥ����˽�breakӦ����ѭ���С�

- ��������

  ```c
  int main()
  {
      int i;
      for(i = 0; i < 100; i++) {
          if (i > 6)
              break;
          print i;
      }
  }
  ```

- ǰ�˱���Ϊָ�

  ![1654607192833](assets/1654607192833.png)

  

  ![1654609516561](assets/1654609516561.png)

  

- ���н��

  ![1654607375473](assets/1654607375473.png)

  

- ����ջ׷��

![1654607471876](assets/1654607471876.png)



### char����

- ˵����ԭ����microcû��char���ͣ��������������һ�����ͣ�������ַ��Ľ�һ��������

- ��������

  ```c
  int main(){
      char i;
      i='a';
      print i;
  }
  ```

- ǰ�˱���Ϊָ�

  ![1654607728619](assets/1654607728619.png)

  

  ![1654609491882](assets/1654609491882.png)

  

  

- ���н��

  ![1654607799159](assets/1654607799159.png)

- ����ջ׷��

![1654607814002](assets/1654607814002.png)



### ��Ŀ���㹦��

- ˵������Ŀ��������ֳ�������������Ǽ�������ԣ�c,c++,java�ȣ�����Ҫ��ɲ��֡�����Ψһ��3�������������������ʱ�ֳ�Ϊ[��Ԫ](https://baike.baidu.com/item/%E4%B8%89%E5%85%83/34063)������� 

  a ? b : c����ⷽʽΪ:

  ```c
  if(a) {
      return b;
  } else {
      return c;
  }
  ```

- ����������

  ```c
  int main()
  {
      int c = 10;
      int b = 8;
      int a = c>b ? c:b;
      print a;
  }
  ```

- ǰ�˱���Ϊָ�

  ![1654608134155](assets/1654608134155.png)

  ?	![1654609459046](assets/1654609459046.png)

  

- ���н��

  ![1654608250618](assets/1654608250618.png)

  

- ����ջ׷��

  ![1654608234433](assets/1654608234433.png)

  

### continue����

- ˵��������������ʾ��������ѭ����������һ��ѭ����������ֹ����ѭ����ִ�С� 

- ��������

  ```c
  int main()
  {
      int i;
      for(i = 0; i < 13; i++) {
          if(i % 2 == 1)
              continue;
          print i;
      }
  }
  ```

  

- ����Ϊָ�

  ![1654608440966](assets/1654608440966.png)

  ![1654608464723](assets/1654608464723.png)

  

- ���н��

  ![1654608497088](assets/1654608497088.png)

  

- ����ջ׷��

![1654608540811](assets/1654608540811.png)



### default����

- ˵������switch-case�Ĺ����У������е�case��û�з�����������������������ʱ����default����ôdefault��Ӧ����䶼��ִ�С��ڳ�����ʹ�øùؼ����ṩһ��Ĭ�ϵķ����� 

- ��������

  ```c
  int main()
  {
      int i = 0;
      int n = 10;
      switch(n) {
          case 0: print i+0;
          case 1: print i+1;
          case 2: print i+2;
          default: print n*n;
               
      }
  }
  ```

- �����ָ�

  ![1654608980065](assets/1654608980065.png)

  

  ![1654609413339](assets/1654609413339.png)

  

- ���н��

  ![1654609024912](assets/1654609024912.png)

  

- ����ջ׷��

  ![1654609049682](assets/1654609049682.png)



### �������ʱ��ֵ����

- ˵������ԭ����microc�У���Ҫ�Ƚ�һ����������Ϊһ������֮�󣬲��ܽ��ñ�����ֵ��������˸ù��ܺ󣬿��������ڶ������ʱ��͸�ֵ��

- ��������

  ```c
  int main()
  {
      int a = 1;
      int b = 3.14;
      int c = 'p';
      print a;
      print b;
      print c;
  }
  ```

- ����Ϊָ�

  ![1654609229120](assets/1654609229120.png)

  

  ![1654609334257](assets/1654609334257.png)

  

- ���н��

  ![1654609348934](assets/1654609348934.png)

  

- ����ջ׷��

  ![1654609369370](assets/1654609369370.png)



### do...While...����

- ˵����do...while ѭ����?[while](https://baike.baidu.com/item/while/755564)?ѭ���ı��塣�ڼ��while()�����Ƿ�Ϊ��֮ǰ����[ѭ��](https://baike.baidu.com/item/%E5%BE%AA%E7%8E%AF/71073)���Ȼ�ִ��һ��do{}֮�ڵ�[���](https://baike.baidu.com/item/%E8%AF%AD%E5%8F%A5/9624168)��Ȼ����while()�ڼ�������Ƿ�Ϊ�棬�������Ϊ��Ļ����ͻ��ظ�do...while���ѭ��,ֱ��while()Ϊ�١� 

- ��������

  ```c
  int main(){
      int n;
      n = 1;
      int a;
      a = 0;
      do{
          a = a + n;
          n = n + 1;
      }while(n<=5);
      print a;
      print n;
  }
  ```

  

- ����Ϊָ�

  ![1654609706081](assets/1654609706081.png)

  ?	

  ![1654609730856](assets/1654609730856.png)

  

- ���н��

  ![1654609800122](assets/1654609800122.png)

  

- ����ջ׷��

  ![1654609814539](assets/1654609814539.png)



### Float����

- ˵������Ƚ���int���ͣ�float����������С��������ʹ�ü������ȷ��

- ����������

  ```c
  int main()
  {
      float a;
      a = 1.1;
      int b;
      b = 2;
      print a+b;
  }
  ```

- ����Ϊָ�

  ![1654609970479](assets/1654609970479.png)

?	![1654609993998](assets/1654609993998.png)



- ���н��

  ![1654610017886](assets/1654610017886.png)

- ����ջ׷��

  ![1654610046586](assets/1654610046586.png)



### Forѭ������

- ˵����forѭ���Ǳ��������һ��ѭ����䣬��[ѭ�����](https://baike.baidu.com/item/%E5%BE%AA%E7%8E%AF%E8%AF%AD%E5%8F%A5/4410586)��[ѭ����](https://baike.baidu.com/item/%E5%BE%AA%E7%8E%AF%E4%BD%93/5125491)��ѭ�����ж�[����](https://baike.baidu.com/item/%E6%9D%A1%E4%BB%B6/1783021)��������ɣ�����ʽΪ��for�����α��ʽ;�������ʽ;ĩβѭ���壩{�м�ѭ���壻}�� 

- ��������

  ```C
  int main()
  {
      int i;
      i = 0;
      int n;
      n = 0;
      for(i =0 ; i < 5 ;  i=i+1){
          n = n + i;
      }
      print n;
  }
  ```

  

- ����Ϊָ�

  ![1654610353638](assets/1654610353638.png)

  ?	![1654610376662](assets/1654610376662.png)

- ���н��

  ![1654610584034](assets/1654610584034.png)

  

- ����ջ׷��

  ![1654610673431](assets/1654610673431.png)

  

### For Range����

- ˵������ָ���ķ�Χ��

- ��������

  ```c
  int main()
  {
      int i;
      for i in (3..9) {
          print i;
      }
  }
  ```

  

- �����ָ�

  ![1654611257360](assets/1654611257360.png)

  

  ![1654611424054](assets/1654611424054.png)

  

- ���н��

  ![1654611488940](assets/1654611488940.png)

- ����ջ׷��

  ![1654611503703](assets/1654611503703.png)



### loopѭ��

- ˵������c���Ե�while���ƣ�����һֱ����ѭ������Ҫ���break��return��ʵ���˳�ѭ�������Դ���forѭ����do...whileѭ����whileѭ���ȡ�

- ����������

  ```c
  int main()
  {
      int i;
      i = 1;
      loop {
          print i;
          if(i == 8)
              break;
          i++;
      }
  }
  ```

- ����Ϊָ�

  ![1654612002381](assets/1654612002381.png)

  

  ![1654612029575](assets/1654612029575.png)

  

- ���н��

  ![1654612197051](assets/1654612197051.png)

  

- ����ջ׷��

  ![1654612213662](assets/1654612213662.png)



### �����Լ�����

- ˵����ʵ���������Լ����ܡ������Լ������������[C](https://baike.baidu.com/item/C/7252092)/[C++](https://baike.baidu.com/item/C%2B%2B/99272)/[C#](https://baike.baidu.com/item/C%23/195147)/[Java](https://baike.baidu.com/item/Java/85979)/��[�߼�����](https://baike.baidu.com/item/%E9%AB%98%E7%BA%A7%E8%AF%AD%E8%A8%80/299113)�У��������������������ǰ��ǰ�������Լ����������󣨺��������Լ����������[����](https://baike.baidu.com/item/%E5%8F%98%E9%87%8F/3956968)��ֵ�ӣ������һ��

- ��������

  ```c
  int main()
  {
    int a;
    int b;
    a=1;
    b=1;
    print a;
    a++;
    print a;
    ++a;
    print a;
    b--;
    print b;
    --b;
    print b;
  }
  ```

  

-  ����Ϊָ�

  ![1654612441775](assets/1654612441775.png)

  ![1654612474677](assets/1654612474677.png)

- ���н��

  ![1654612495238](assets/1654612495238.png)

  

- ����ջ׷��

  ![1654612514589](assets/1654612514589.png)



## 5.��Ŀ��������

| ����            | ��Ӧ�ļ�    | ��   | ��   | ��   |
| --------------- | ----------- | ---- | ---- | ---- |
| switch-case���� | switch.c    | ��    |      |      |
| break����       | break.c     | ��    |      |      |
| char����        | char.c      |      | ��    |      |
| ��Ŀ���㹦��    | condition.c | ��    |      |      |
| continue����    | continue.c  | ��    |      |      |
| default����     | default.c   | ��    |      |      |
| ���������ֵ    | define.c    |      |      |      |
| do..while����   | doWhile.c   | ��    |      |      |
| Float����       | float.c     | ��    |      |      |
| Forѭ��         | for.c       | ��    |      |      |
| For in Range    | forRange.c  |      | ��    |      |
| Loopѭ��        | loop.c      | ��    |      |      |
| �����Լ�����    | selfPlus.c  | ��    |      |      |









