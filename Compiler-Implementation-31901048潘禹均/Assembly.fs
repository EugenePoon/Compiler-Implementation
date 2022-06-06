module Assembly

type label = string
//指令
type instruction =
    | Label of label    //象征性的标签
    | CSTI of int32     //int常数     
    | CSTF of int32     //
    | CSTC of int32     //
    | ADD               // + 
    | SUB               // -
    | MUL               // *
    | DIV               // /
    | MOD               // %
    | EQ                // ==
    | LT                // <
    | NOT               // !=
    | DUP               //复制栈顶
    | SWAP              //交换
    | LDI               //取地址的值，SP装当前地址所在的下标，s[sp]为当前地址，s[s[sp]]为地址的值
    | STI               //set s[s[sp-1]] sp在栈顶，sp-1为栈顶下面一个下标，s[sp-1]取栈顶下面一个地方的值,为地址，赋值，s[sp]是新进来的值
    | GETBP             // 得到BP：栈基址寄存器，栈寻址寄存器
    | GETSP             // 得到栈指针，栈顶指针寄存器
    | INCSP of int      // 增加堆栈顶部
    | GOTO of label     // go to label
    | IFZERO of label   // go to label if s[sp] == 0
    | IFNZRO of label   // go to label if s[sp] != 0
    | CALL of int * label  //向上移动m个参数1，推进pc，跳跃,调用函数
    | TCALL of int * int * label  //向下移动m个参数，推进pc，调用函数
    | RET of int            //弹出参数，返回到栈顶
    | PRINTI                //输出栈顶 int形式
    | PRINTC                //输出栈顶，char形式
    | LDARGS                //在堆栈上加载命令行参数
    | STOP                  //停止抽象语法树栈式虚拟机
    | THROW of int          //抛出异常
    | PUSHHDLR of int * label    //PUSH
    | POPHDLR                    //POP
    | SLEEP

//返回两个函数 restetLabels，newLabel
let (resetLabels, newLabel) =
    let lastlab = ref -1
    ((fun () -> lastlab := 0), (fun () -> (lastlab := 1 + !lastlab; "L" + (!lastlab).ToString())))
//简单的环境操作
type 'data env = (string * 'data) list

// 遍历环境字典中寻找x
let rec lookup env x = 
    match env with
    | []            -> failwith(x + "not found")
    | (y, v)::yr    -> if x=y then v else lookup yr x

[<Literal>]
let CODECSTI    = 0

[<Literal>]
let CODEADD     = 1

[<Literal>]
let CODESUB     = 2

[<Literal>]
let CODEMUL     = 3

[<Literal>]
let CODEDIV     = 4

[<Literal>]
let CODEMOD     = 5

[<Literal>]
let CODEEQ      = 6

[<Literal>]
let CODELT      = 7

[<Literal>]
let CODENOT     = 8

[<Literal>]
let CODEDUP     = 9

[<Literal>]
let CODESWAP    = 10

[<Literal>]
let CODELDI     = 11

[<Literal>]
let CODESTI     = 12

[<Literal>]
let CODEGETBP   = 13

[<Literal>]
let CODEGETSP   = 14

[<Literal>]
let CODEINCSP   = 15

[<Literal>]
let CODEGOTO    = 16

[<Literal>]
let CODEIFZERO  = 17

[<Literal>]
let CODEIFNZRO  = 18

[<Literal>]
let CODECALL    = 19

[<Literal>]
let CODETCALL   = 20

[<Literal>]
let CODERET     = 21

[<Literal>]
let CODEPRINTI  = 22

[<Literal>]
let CODEPRINTC  = 23

[<Literal>]
let CODELDARGS  = 24

[<Literal>]
let CODESTOP    = 25;


[<Literal>]
let CODECSTF    = 26;

[<Literal>]
let CODECSTC    = 27;

[<Literal>]
let CODETHROW   = 28;

[<Literal>]
let CODEPUSHHR  = 29;

[<Literal>]
let CODEPOPHR   = 30;

[<Literal>]
let CODESLEEP   = 31

// 获得标签在机器码中的地址
// 记录当前(标签，地址) ==> 到labenv中
// 没有参数，加一个地址，每多一个参数，加一个地址
let makelabenv (addr, labenv) instruction = 
    match instruction with
    | Label lab         -> (addr, (lab, addr) :: labenv)
    | CSTI i            -> (addr+2, labenv)
    | CSTF i            -> (addr+2, labenv)
    | CSTC i            -> (addr+2, labenv)
    | ADD               -> (addr+1, labenv)
    | SUB               -> (addr+1, labenv)
    | MUL               -> (addr+1, labenv)
    | DIV               -> (addr+1, labenv)
    | MOD               -> (addr+1, labenv)
    | EQ                -> (addr+1, labenv)
    | LT                -> (addr+1, labenv)
    | NOT               -> (addr+1, labenv)
    | DUP               -> (addr+1, labenv)
    | SWAP              -> (addr+1, labenv)
    | LDI               -> (addr+1, labenv)
    | STI               -> (addr+1, labenv)
    | GETBP             -> (addr+1, labenv)
    | GETSP             -> (addr+1, labenv)
    | INCSP m           -> (addr+2, labenv)
    | GOTO lab          -> (addr+2, labenv)
    | IFZERO lab        -> (addr+2, labenv)
    | IFNZRO lab        -> (addr+2, labenv)
    | CALL(m, lab)      -> (addr+3, labenv)
    | TCALL(m, n, lab)  -> (addr+4, labenv)
    | RET m             -> (addr+2, labenv)
    | PRINTI            -> (addr+1, labenv)
    | PRINTC            -> (addr+1, labenv)
    | LDARGS            -> (addr+1, labenv)
    | STOP              -> (addr+1, labenv)
    | THROW i           -> (addr+2, labenv)
    | PUSHHDLR (exn ,lab) -> (addr+3, labenv)
    | POPHDLR           -> (addr+1, labenv)
    | SLEEP             -> (addr+1, labenv)

// getlab是得到标签所在地址的函数
// let getlab lab = lookup labenv lab
let rec emitints getlab instruction ints = 
    match instruction with
    | Label lab         -> ints
    | CSTI i            -> CODECSTI     :: i            :: ints
    | CSTF i            -> CODECSTF     :: i            :: ints
    | CSTC i            -> CODECSTC     :: i            :: ints
    | ADD               -> CODEADD      :: ints
    | SUB               -> CODESUB      :: ints
    | MUL               -> CODEMUL      :: ints
    | DIV               -> CODEDIV      :: ints
    | MOD               -> CODEMOD      :: ints
    | EQ                -> CODEEQ       :: ints
    | LT                -> CODELT       :: ints
    | NOT               -> CODENOT      :: ints
    | DUP               -> CODEDUP      :: ints
    | SWAP              -> CODESWAP     :: ints
    | LDI               -> CODELDI      :: ints
    | STI               -> CODESTI      :: ints
    | GETBP             -> CODEGETBP    :: ints
    | GETSP             -> CODEGETSP    :: ints
    | INCSP m           -> CODEINCSP    :: m            :: ints
    | GOTO lab          -> CODEGOTO     :: getlab lab   :: ints
    | IFZERO lab        -> CODEIFZERO   :: getlab lab   :: ints
    | IFNZRO lab        -> CODEIFNZRO   :: getlab lab   :: ints
    | CALL(m, lab)      -> CODECALL     :: m            :: getlab lab   :: ints
    | TCALL(m, n, lab)  -> CODETCALL    :: m            :: n            :: getlab lab   :: ints
    | RET m             -> CODERET      :: m            :: ints
    | PRINTI            -> CODEPRINTI   :: ints
    | PRINTC            -> CODEPRINTC   :: ints
    | LDARGS            -> CODELDARGS   :: ints
    | STOP              -> CODESTOP     :: ints
    | SLEEP            -> CODESLEEP     :: ints
    | THROW i           -> CODETHROW    :: i            :: ints
    | PUSHHDLR (exn, lab) -> CODEPUSHHR :: exn          :: getlab lab   :: ints
    | POPHDLR           -> CODEPOPHR    :: ints

//通过对 code 的两次遍历,完成汇编指令到机器指令的转换
let code2ints (code : instruction list) : int list = 
    // 从前往后遍历汇编指令序列
    let (_, labenv) = List.fold makelabenv (0, []) code
    //  getlab是得到标签所在地址的函数
    let getlab lab = lookup labenv lab
    // 从后向前遍历汇编指令序列
    List.foldBack (emitints getlab) code []


let ntolabel (n:int) :label =
    string(n)

// 反编译
let rec decomp ints : instruction list = 

    // printf "%A" ints

    match ints with
    | []                                            ->   []
    | CODEADD :: ints_rest                          ->   ADD            :: decomp ints_rest
    | CODESUB    :: ints_rest                       ->   SUB            :: decomp ints_rest
    | CODEMUL    :: ints_rest                       ->   MUL            :: decomp ints_rest
    | CODEDIV    :: ints_rest                       ->   DIV            :: decomp ints_rest
    | CODEMOD    :: ints_rest                       ->   MOD            :: decomp ints_rest
    | CODEEQ     :: ints_rest                       ->   EQ             :: decomp ints_rest
    | CODELT     :: ints_rest                       ->   LT             :: decomp ints_rest
    | CODENOT    :: ints_rest                       ->   NOT            :: decomp ints_rest
    | CODEDUP    :: ints_rest                       ->   DUP            :: decomp ints_rest
    | CODESWAP   :: ints_rest                       ->   SWAP           :: decomp ints_rest
    | CODELDI    :: ints_rest                       ->   LDI            :: decomp ints_rest
    | CODESTI    :: ints_rest                       ->   STI            :: decomp ints_rest
    | CODEGETBP  :: ints_rest                       ->   GETBP          :: decomp ints_rest
    | CODEGETSP  :: ints_rest                       ->   GETSP          :: decomp ints_rest
    | CODEINCSP  :: m :: ints_rest                  ->   INCSP m        :: decomp ints_rest
    | CODEGOTO   :: lab :: ints_rest                ->   GOTO (ntolabel lab)        :: decomp ints_rest
    | CODEIFZERO :: lab :: ints_rest                ->   IFZERO (ntolabel lab)      :: decomp ints_rest
    | CODEIFNZRO :: lab :: ints_rest                ->   IFNZRO (ntolabel lab)      :: decomp ints_rest
    | CODECALL   :: m :: lab :: ints_rest           ->   CALL(m, ntolabel lab)      :: decomp ints_rest
    | CODETCALL  :: m :: n ::  lab :: ints_rest     ->   TCALL(m,n,ntolabel lab)    :: decomp ints_rest
    | CODERET    :: m :: ints_rest                  ->   RET m          :: decomp ints_rest
    | CODEPRINTI :: ints_rest                       ->   PRINTI         :: decomp ints_rest
    | CODEPRINTC :: ints_rest                       ->   PRINTC         :: decomp ints_rest
    | CODELDARGS :: ints_rest                       ->   LDARGS         :: decomp ints_rest
    | CODESTOP   :: ints_rest                       ->   STOP           :: decomp ints_rest
    | CODECSTI   :: i :: ints_rest                  ->   CSTI i         :: decomp ints_rest       
    | CODECSTF   :: i :: ints_rest                  ->   CSTF i         :: decomp ints_rest    
    | CODECSTC   :: i :: ints_rest                  ->   CSTC i         :: decomp ints_rest        
    | _                                             ->    printf "%A" ints; failwith "unknow code"

