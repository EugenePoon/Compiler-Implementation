import mytype.*;
import exception.*;

import java.io.*;
import java.util.ArrayList;
// 模式匹配
import java.util.regex.Pattern;

public class Machine extends Thread{
    // 规定堆栈的大小
    private final static int STACKSIZE = 1000;

        public static void main(String[] args) throws FileNotFoundException, IOException, OperatorError, TypeError,InterruptedException {
        // 参数长度
        if (args.length == 0)
            System.out.println("Usage: java Machine <programfile> <arg1> ...\n");
        else {
            // 运行.out，这是一个字符指令集，后面跟进入的参数
            execute(args, false);
        }
    }

    static void execute(String[] args, boolean trace) throws FileNotFoundException, IOException, OperatorError, TypeError,InterruptedException {
        // 读程序从文件中，args[0]为文件名，为*.out
        ArrayList<Integer> program = readfile(args[0]);
        // 堆栈
        basicType[] stack = new basicType[STACKSIZE];
        // 参数列表
        basicType[] inputArgs = new basicType[args.length - 1];

        for (int i = 1; i < args.length; i++) {
            // 只要arg[i]中匹配到了(?i)[a-z]就返回true，一个个参数进去
            // 字符参数
            if(Pattern.compile("(?i)[a-z]").matcher(args[i]).find()){
                char[] input = args[i].toCharArray();
                CharType[] array = new CharType[input.length];
                for(int j = 0; j < input.length; ++j) {
                    array[j] = new CharType(input[j]);
                }
                // 参数数组
                inputArgs[i-1] = new ArrayType(array);
            }
            // float参数
            else if(args[i].contains(".")){
                inputArgs[i-1] = new FloatType(new Float(args[i]).floatValue());
            }
            // Int参数
            else {
                inputArgs[i-1] = new IntType(new Integer(args[i]).intValue());
            }
        }
        long startTime = System.currentTimeMillis();
        // 具体编译过程，程序，
        // program为.out的数字指令集，inputArgs为参数
        execCode(program, stack, inputArgs, trace);
        long runtime = System.currentTimeMillis() - startTime;
        System.err.println("\nRan " + runtime/1000.0 + " seconds");
    }

    // 进入时，程序已成为数字指令集
    private static int execCode(ArrayList<Integer> program, basicType[] stack, basicType[] inputArgs, boolean trace) throws TypeError, OperatorError,InterruptedException{
        int bp = -999;  //基址指针，方便本地变量的访问
        int sp = -1;    //栈顶指针
        int pc = 0;     //程序计数器，下一条指令载入
        int hr = -1;
        for (;;) {
            if (trace)
                // 后续代码
                // program为.out的数字指令集，inputArgs为参数
                printSpPc(stack, bp, sp, program, pc);
            switch (program.get(pc++)) {
                // 类型
                case Instruction.CSTI: //若类型声明为int，将下一个的int值取出，入栈
                    stack[sp + 1] = new IntType(program.get(pc++)); sp++; break;
                case Instruction.CSTF:  //int字节转换为FLoat 入栈
                    stack[sp + 1] = new FloatType(Float.intBitsToFloat(program.get(pc++))); sp++; break;
                case Instruction.CSTC:  //ASKII码直接转换为char
                    stack[sp + 1] = new CharType((char)(program.get(pc++).intValue())); sp++; break;
                // 操作
                case Instruction.ADD: { // 3 4 +，此时栈顶的值为4，4出栈，3为sp-1处，3处变为7
                    // if((String.valueOf(stack[sp-1])).indexOf(String.valueOf("."))!=-1){
                    //     float x = Float.parseFloat(String.valueOf(stack[sp-1])) + Float.parseFloat(String.valueOf(stack[sp]));
                    //     FloatType ft = new FloatType(x);
                    //     stack[sp-1] = ft;
                    //     sp--;
                    // }
                    // else{
                    //     int x = Integer.parseInt(String.valueOf(stack[sp-1])) + Integer.parseInt(String.valueOf(stack[sp]));
                    //     IntType it = new IntType(x);
                    //     stack[sp-1] = it;
                    //     sp--;
                    // }
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "+");
                    sp--; //后一个数字出栈
                    break;
                }
                case Instruction.SUB:{
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "-");
                    sp--; //后一个数字出栈
                    break;
                }

                case Instruction.MUL: {
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "*");
                    sp--;
                    break;
                }
                // hr的存在不懂，//hr是上一步的地方 ？
                //增加异常处理
                case Instruction.DIV: //3 4 / 保证4所表示的不为0
                    int flag = 0;
                    if(stack[sp] instanceof FloatType){
                        if(((FloatType)stack[sp]).getValue()==0){
                            flag = 1;
                        }
                    }
                    else if(stack[sp] instanceof IntType){
                        if(((IntType)stack[sp]).getValue()==0){
                            flag = 1;
                        }
                    }
                    if(flag==1)
                    {
                        System.out.println("hr:"+hr+" exception:"+1);
                        while (hr != -1 && ((IntType)stack[hr]).getValue() != 1 )
                        {
                            hr = ((IntType)stack[hr+2]).getValue();
                            System.out.println("hr:"+hr+" exception:"+new IntType(program.get(pc)).getValue());
                        }
                            // 返回到调用处，追溯异常
                        if (hr != -1) { 
                            sp = hr-1;    
                            pc = ((IntType)stack[hr+1]).getValue();
                            hr = ((IntType)stack[hr+2]).getValue();    
                        } else {
                            System.out.print(hr+"not find exception");
                            return sp;
                        }
                    }
                    else{
                        stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "/");
                        sp--; 
                    }
                    
                    break;
                case Instruction.MOD:
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "%");
                    sp--;
                    break;
                case Instruction.EQ:
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "==");
                    sp--;
                    break;
                case Instruction.LT:
                    stack[sp - 1] = binaryOperator(stack[sp-1], stack[sp], "<");
                    sp--;
                    break;
                case Instruction.NOT: {
                    Object result = null;
                    if(stack[sp] instanceof FloatType){
                        result = ((FloatType)stack[sp]).getValue();
                    }else if (stack[sp] instanceof IntType){
                        result = ((IntType)stack[sp]).getValue();
                    }
                    stack[sp] = (Float.compare(new Float(result.toString()), 0.0f) == 0 ? new IntType(1) : new IntType(0));
                    break;
                }
                case Instruction.DUP: //复制
                    stack[sp+1] = stack[sp];
                    sp++;
                    break;
                case Instruction.SWAP: { //交换
                    basicType tmp = stack[sp];  stack[sp] = stack[sp-1];  stack[sp-1] = tmp;
                    break;
                }
                case Instruction.LDI: // load indirect 间接取值//取地址的值，SP装当前地址所在的下标，s[sp]为当前地址，s[s[sp]]为地址的值
                    stack[sp] = stack[((IntType)stack[sp]).getValue()]; break;
                case Instruction.STI: //store indirect, keep value on top，间接存储，保持值在最上方set s[s[sp-1]] 
                    stack[((IntType)stack[sp-1]).getValue()] = stack[sp]; stack[sp-1] = stack[sp]; sp--; break;
                case Instruction.GETBP:
                    stack[sp+1] = new IntType(bp); sp++; break;
                case Instruction.GETSP:
                    stack[sp+1] = new IntType(sp); sp++; break;
                case Instruction.INCSP: //增大空间？
                    sp = sp + program.get(pc++); break;
                case Instruction.GOTO: //到哪一个代码去，pc等于当前的指令顺序
                    pc = program.get(pc); break;
                case Instruction.IFZERO: { //
                    Object result = null;
                    int index = sp--;
                    // 判断类型
                    if(stack[index] instanceof IntType){
                        result = ((IntType)stack[index]).getValue();
                    }else if(stack[index] instanceof FloatType){
                        result = ((FloatType)stack[index]).getValue();
                    }
                    // 是0就当前的指令，不是0就下一条指令
                    pc = (Float.compare(new Float(result.toString()), 0.0f) == 0 ? program.get(pc) : pc + 1);
                    break;
                }
                case Instruction.IFNZRO: {
                    Object result = null;
                    int index = sp--;
                    if (stack[index] instanceof IntType) {
                        result = ((IntType) stack[index]).getValue();
                    } else if (stack[index] instanceof FloatType) {
                        result = ((FloatType) stack[index]).getValue();
                    }
                    //  不是0就当前的指令，不是0就下一条指令
                    pc = (Float.compare(new Float(result.toString()), 0.0f) != 0 ? program.get(pc) : pc + 1);
                    break;
                }
                //跳转
                case Instruction.CALL: {
                    // argc为跳转地址
                    int argc = program.get(pc++);
                    // 跳转程序进栈
                    for (int i=0; i<argc; i++)  
                        stack[sp-i+2] = stack[sp-i];  //为回信地址保留空间，旧的基本指针
                    stack[sp-argc+1] = new IntType(pc+1); sp++; 
                    stack[sp-argc+1] = new IntType(bp);   sp++;
                    bp = sp+1-argc;
                    pc = program.get(pc);
                    break;
                }
                // 跳转并返回，函数出栈
                case Instruction.TCALL: {
                    int argc = program.get(pc++);       //新的参数
                    int pop  = program.get(pc++);       //要丢弃的变量数
                    for (int i=argc-1; i>=0; i--)       //丢弃变量
                        stack[sp-i-pop] = stack[sp-i];
                    sp = sp - pop; 
                    pc = program.get(pc);
                    break;
                } 
                case Instruction.RET: {
                    basicType res = stack[sp];
                    sp = sp - program.get(pc); 
                    bp = ((IntType)stack[--sp]).getValue(); 
                    pc = ((IntType)stack[--sp]).getValue();
                    stack[sp] = res;
                    break;
                } 
                case Instruction.PRINTI: {
                    Object result;
                    if(stack[sp] instanceof IntType){
                        result = ((IntType)stack[sp]).getValue();
                    }else if(stack[sp] instanceof FloatType){
                        result = ((FloatType)stack[sp]).getValue();
                    }else if(stack[sp] instanceof StringType){
                        result = ((StringType)stack[sp]).getValue();
                    }else{
                        result = ((StringType)stack[sp]).getValue();
                    }

                    System.out.print(String.valueOf(result) + " ");
                    break;
                }
                case Instruction.SLEEP: {
                   long result;
                    if(stack[sp] instanceof IntType){
                        result = ((IntType)stack[sp]).getValue();
                    }else if(stack[sp] instanceof FloatType){
                        throw new TypeError("TypeError: sleep args is not int");
                    }else {
                        throw new TypeError("TypeError: sleep args is not int");
                    }
                    Thread.sleep(result);
                    break;
                }
                case Instruction.PRINTC:
                    System.out.print((((CharType)stack[sp])).getValue()); break;
                case Instruction.LDARGS: //命令行参数入栈
                    for (int i=0; i < inputArgs.length; i++) // Push commandline arguments
                        stack[++sp] = inputArgs[i];
                    break;
                case Instruction.STOP:
                    return sp;
                // HR用于异常时回溯
                case Instruction.PUSHHR:{
                    stack[++sp] = new IntType(program.get(pc++));    //exn
                    int tmp = sp;       //exn address
                    sp++;
                    stack[sp++] = new IntType(program.get(pc++));   //jump address
                    stack[sp] = new IntType(hr);
                    hr = tmp;
                    break;
                }
                case Instruction.POPHR:
                    hr = ((IntType)stack[sp--]).getValue();sp-=2;break;
                case Instruction.THROW:
                    System.out.println("hr:"+hr+" exception:"+new IntType(program.get(pc)).getValue());
                    // ((IntType)stack[hr]).getValue() == program.get(pc) )已经是最初调用的
                    while (hr != -1 && ((IntType)stack[hr]).getValue() != program.get(pc) )
                    {
                        // stack[hr+2]之前调用的跳转地址，一直追溯到最前面
                        hr = ((IntType)stack[hr+2]).getValue(); //find exn address
                        System.out.println("hr:"+hr+" exception:"+new IntType(program.get(pc)).getValue());
                    }
                        
                    if (hr != -1) { // Found a handler for exn
                        sp = hr-1;    // remove stack after hr，已经追溯完的出栈
                        pc = ((IntType)stack[hr+1]).getValue(); //next instruction
                        hr = ((IntType)stack[hr+2]).getValue(); // with current handler being hr当前处理程序为hr     
                    } else {
                        System.out.print(hr+"not find exception");
                        return sp;
                    }break;

                default:
                    throw new RuntimeException("Illegal instruction " + program.get(pc-1)
                            + " at address " + (pc-1));

            }


        }


    }

public static basicType binaryOperator(basicType lhs, basicType rhs, String operator) throws TypeError, OperatorError {
        Object left;
        Object right;
        int flag = 0;
        // 判断左右值的类型
        if (lhs instanceof FloatType) {
            left = ((FloatType) lhs).getValue();
            flag = 1;
        } else if (lhs instanceof IntType) {
            left = ((IntType) lhs).getValue();
        } else if (operator=="+"&&lhs instanceof CharType) {
            left = (((CharType) lhs).getValue());
            flag = 2;
        } else if (operator=="+"&&lhs instanceof StringType){
            left = (((StringType)lhs).getValue());
            flag = 3;
        }
        else {
            throw new TypeError("TypeError: Left type is not int or float");
        }
        if (rhs instanceof FloatType) {
            right = ((FloatType) rhs).getValue();;
            flag = 1;
        } else if (rhs instanceof IntType) {
            right = ((IntType) rhs).getValue();
        }  else if (operator=="+"&&rhs instanceof CharType) {
            right = (((CharType) rhs).getValue());
            flag = 2;
        } else if (operator=="+"&&rhs instanceof StringType){
            right = (((StringType)rhs).getValue());
            flag = 3;
        } else {
            throw new TypeError("TypeError: Right type is not int or float");
        }
        basicType result = null;

        switch(operator){
            case "+":{
                if (flag == 1) {
                    result =  new FloatType(Float.parseFloat(String.valueOf(left)) + Float.parseFloat(String.valueOf(right)));
                } else if(flag == 2){
                    StringType achar = new StringType((char)left);
                    String astring = achar.addChar(String.valueOf(right));
                    result = new StringType(astring);
                    // System.out.println("4:"+result);
                } else if(flag == 3){
                    StringType astring = new StringType(String.valueOf(left));
                    String astrings = astring.addChar(String.valueOf(right));
                    result = new StringType(astrings);
                } else {
                    result = new IntType(Integer.parseInt(String.valueOf(left)) + Integer.parseInt(String.valueOf(right)));
                }
                break;
            }
            case "-":{
                if (flag == 1) {
                    result = new FloatType(Float.parseFloat(String.valueOf(left)) - Float.parseFloat(String.valueOf(right)));
                } else {
                    result = new IntType(Integer.parseInt(String.valueOf(left)) - Integer.parseInt(String.valueOf(right)));
                }
                break;
            }
            case "*":{
                if (flag == 1) {
                    result = new FloatType(Float.parseFloat(String.valueOf(left)) * Float.parseFloat(String.valueOf(right)));
                } else {
                    result = new IntType(Integer.parseInt(String.valueOf(left)) * Integer.parseInt(String.valueOf(right)));
                }
                break;
            }
            case "/":{
                if(Float.compare(Float.parseFloat(String.valueOf(right)), 0.0f) == 0){
                    throw new OperatorError("OpeatorError: Divisor can't not be zero");
                }
                if (flag == 1) {
                    result = new FloatType(Float.parseFloat(String.valueOf(left)) / Float.parseFloat(String.valueOf(right)));
                } else {
                    result = new IntType(Integer.parseInt(String.valueOf(left)) / Integer.parseInt(String.valueOf(right)));
                }
                break;
            }
            case "%":{
                if (flag == 1) {
                    throw new OperatorError("OpeatorError: Float can't mod");
                } else {
                    result = new IntType(Integer.parseInt(String.valueOf(left)) % Integer.parseInt(String.valueOf(right)));
                }
                break;
            }
            case "==":{
                if (flag == 1) {
                    if((float) left == (float) right){
                        result = new IntType(1);
                    }
                    else{
                        result = new IntType(0);
                    }
                } else {
                    if((int) left == (int) right){
                        result = new IntType(1);
                    }
                    else{
                        result = new IntType(0);
                    }
                }
                break;
            }
            case "<":{
                if (flag == 1) {
                    if((float) left < (float) right){
                        result = new IntType(1);
                    }
                    else{
                        result = new IntType(0);
                    }
                } else {
                    if((int) left < (int) right){
                        result = new IntType(1);
                    }
                    else{
                        result = new IntType(0);
                    }
                }
                break;
            }
        }
        return result;
    }


    private static String insName(ArrayList<Integer> program, int pc) {
        switch (program.get(pc)) {
            case Instruction.CSTI:   return "CSTI " + program.get(pc+1);
            case Instruction.CSTF:   return "CSTF " + program.get(pc+1);
            case Instruction.CSTC:   return "CSTC " + (char)(program.get(pc+1).intValue());
            case Instruction.ADD:    return "ADD";
            case Instruction.SUB:    return "SUB";
            case Instruction.MUL:    return "MUL";
            case Instruction.DIV:    return "DIV";
            case Instruction.MOD:    return "MOD";
            case Instruction.EQ:     return "EQ";
            case Instruction.LT:     return "LT";
            case Instruction.NOT:    return "NOT";
            case Instruction.DUP:    return "DUP";
            case Instruction.SWAP:   return "SWAP";
            case Instruction.LDI:    return "LDI";
            case Instruction.STI:    return "STI";
            case Instruction.GETBP:  return "GETBP";
            case Instruction.GETSP:  return "GETSP";
            case Instruction.INCSP:  return "INCSP " + program.get(pc+1);
            case Instruction.GOTO:   return "GOTO " + program.get(pc+1);
            case Instruction.IFZERO: return "IFZERO " + program.get(pc+1);
            case Instruction.IFNZRO: return "IFNZRO " + program.get(pc+1);
            case Instruction.CALL:   return "CALL " + program.get(pc+1) + " " + program.get(pc+2);
            case Instruction.TCALL:  return "TCALL " + program.get(pc+1) + " " + program.get(pc+2) + " " +program.get(pc+3);
            case Instruction.RET:    return "RET " + program.get(pc+1);
            case Instruction.PRINTI: return "PRINTI";
            case Instruction.SLEEP:  return "SLEEP";
            case Instruction.PRINTC: return "PRINTC";
            case Instruction.LDARGS: return "LDARGS";
            case Instruction.STOP:   return "STOP";
            case Instruction.THROW:  return "THROW" + program.get(pc+1);
            case Instruction.PUSHHR: return "PUSHHR" + " " + program.get(pc+ 1) + " " + program.get(pc+2) ;
            case Instruction.POPHR: return "POPHR";
            default:     return "<unknown>";
        }
    }


    private static void printSpPc(basicType[] stack, int bp, int sp, ArrayList<Integer> program, int pc) {
        System.out.print("[ ");
        for (int i = 0; i <= sp; i++) {
            Object result = null;
            if(stack[i] instanceof IntType){
                result = ((IntType)stack[i]).getValue();
            }else if(stack[i] instanceof FloatType){
                result = ((FloatType)stack[i]).getValue();
            }else if(stack[i] instanceof CharType){
                result = ((CharType)stack[i]).getValue();
            }
            System.out.print(String.valueOf(result) + " ");
        }
        System.out.print("]");
        System.out.println("{" + pc + ": " + insName(program, pc) + "}");
    }


    private static ArrayList<Integer> readfile(String filename) throws FileNotFoundException, IOException {
        ArrayList<Integer> program = new ArrayList<Integer>();
        Reader inp = new FileReader(filename);

        StreamTokenizer tStream = new StreamTokenizer(inp);
        tStream.parseNumbers();
        tStream.nextToken();
        while (tStream.ttype == StreamTokenizer.TT_NUMBER) {
            program.add(new Integer((int)tStream.nval));
            tStream.nextToken();
        }

        inp.close();

        return program;
    }
}



// 产生machinetrace.class
class Machinetrace{
    public static void main(String[] args)
            throws FileNotFoundException, IOException, OperatorError, TypeError,InterruptedException {
        if (args.length == 0)
            System.out.println("Usage: java Machinetrace <programfile> <arg1> ...\n");
        else
            Machine.execute(args, true);
    }
}
