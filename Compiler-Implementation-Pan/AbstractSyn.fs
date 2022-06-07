
module AbstractSyn

// 基本类型
// 注意，数组、指针是递归类型

type typ = 
  | TypInt
  | TypChar
  | TypString
  | TypFloat
  | TypVoid
  | TypArray of typ * int option
  | TypPoint of typ
  | Lambda of typ option * (typ * string) list * statement
 
and expr =                                // 表达式，右值 
  | Access of access
  | Assign of access * expr
  | Addr of access
  | CstInt of int
  | CstFloat of float32
  | CstString of string
  | CstChar of char
  | CstNull of int
  | NullExpression of int
  | UnaryPrim of string * expr
  | BinaryPrim of string * expr * expr
  | TernaryPrim of expr * expr * expr
  | Andalso of expr * expr
  | Orelse of expr * expr
  | Call of string * expr list

and access =                               //左值，存储的位置  
  | AccVar of string
  | AccDeref of expr
  | AccIndex of access * expr

and statement = 
  | If of expr * statement * statement
  | While of expr * statement
  | DoWhile of statement * expr
  | Expr of expr
  | Return of expr option
  | Block of statementDec list
  | For of expr * expr * expr * statement
  | Loop of statement
  | Default of statement
  | Case of expr * statement
  | Switch of expr * statement list
  | Range of expr * expr * expr * statement
  | Break
  | Continue
  | Sleep of expr
// 语句块内部，可以是变量声明 或语句的列表 
and statementDec = 
  | Dec of typ * string
  | DecAsg of typ * string * expr           //变量赋值
  | Stmt of statement   

// 顶级声明 可以是函数声明或变量声明
and topDec = 
  | FunDec of typ option * string * (typ * string) list * statement
  | VarDec of typ * string
  | VarDecAsg of typ * string * expr        //变量赋值
 
// 程序是顶级声明的列表
and program = 
  | Prog of topDec list