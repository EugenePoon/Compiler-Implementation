
module AbstractSyn

type typ = 
  | TypInt
  | TypChar
  | TypString
  | TypFloat
  | TypVoid
  | TypArray of typ * int option
  | TypPoint of typ
  | Lambda of typ option * (typ * string) list * statement
 
and expr =
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

and access = 
  | AccVar of string
  | AccDeref of expr
  | AccIndex of access * expr

and statement = 
  | If of expr * statement * statement
  | While of expr * statement
  | DoWhile of statement * expr
  | DoUntil of stmt * expr
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

and statementDec = 
  | Dec of typ * string
  | DecAsg of typ * string * expr
  | Stmt of statement

and topDec = 
  | FunDec of typ option * string * (typ * string) list * statement
  | VarDec of typ * string
  | VarDecAsg of typ * string * expr
 
and program = 
  | Prog of topDec list