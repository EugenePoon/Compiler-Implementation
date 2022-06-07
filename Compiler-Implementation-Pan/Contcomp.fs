
module Contcomp
open System.IO
open AbstractSyn
open Machine


type bstmtordec =
     | BDec of instrs list               
     | BStmt of statement                  



let rec addINCSP m1 C : instrs list =
    match C with
    | INCSP m2            :: C1 -> addINCSP (m1+m2) C1
    | RET m2              :: C1 -> RET (m2-m1) :: C1
    | Label lab :: RET m2 :: _  -> RET (m2-m1) :: C
    | _                         -> if m1=0 then C else INCSP m1 :: C

let addLabel C : label * instrs list =            // 条件跳转
    match C with
    | Label lab :: _ -> (lab, C)
    | GOTO lab :: _  -> (lab, C)
    | _              -> let lab = newLabel() 
                        (lab, Label lab :: C)

let makeJump C : instrs * instrs list =          // 无条件跳转
    match C with
    | RET m              :: _ -> (RET m, C)
    | Label lab :: RET m :: _ -> (RET m, C)
    | Label lab          :: _ -> (GOTO lab, C)
    | GOTO lab           :: _ -> (GOTO lab, C)
    | _                       -> let lab = newLabel() 
                                 (GOTO lab, Label lab :: C)

let makeCall m lab C : instrs list =
    match C with
    | RET n            :: C1 -> TCALL(m, n, lab) :: C1
    | Label _ :: RET n :: _  -> TCALL(m, n, lab) :: C
    | _                      -> CALL(m, lab) :: C

let rec deadcode C =
    match C with
    | []              -> []
    | Label lab :: _  -> C
    | _         :: C1 -> deadcode C1

let addNOT C =
    match C with
    | NOT        :: C1 -> C1
    | IFZERO lab :: C1 -> IFNZRO lab :: C1 
    | IFNZRO lab :: C1 -> IFZERO lab :: C1 
    | _                -> NOT :: C

let addJump jump C =          
    let C1 = deadcode C
    match (jump, C1) with
    | (GOTO lab1, Label lab2 :: _) -> if lab1=lab2 then C1 
                                      else GOTO lab1 :: C1
    | _                            -> jump :: C1
    
let addGOTO lab C =
    addJump (GOTO lab) C

let rec addCST i C =
    match (i, C) with
    | (0, ADD        :: C1) -> C1
    | (0, SUB        :: C1) -> C1
    | (0, NOT        :: C1) -> addCST 1 C1
    | (_, NOT        :: C1) -> addCST 1 C1
    | (1, MUL        :: C1) -> C1
    | (1, DIV        :: C1) -> C1
    | (0, EQ         :: C1) -> addNOT C1
    | (_, INCSP m    :: C1) -> if m < 0 then addINCSP (m+1) C1
                               else CSTI i :: C
    | (0, IFZERO lab :: C1) -> addGOTO lab C1
    | (_, IFZERO lab :: C1) -> C1
    | (0, IFNZRO lab :: C1) -> C1
    | (_, IFNZRO lab :: C1) -> addGOTO lab C1
    | _                     -> CSTI i :: C

let rec addCSTF i C = 
    match (i, C) with
    | _                     -> (CSTF (System.BitConverter.SingleToInt32Bits(float32(i)))) :: C

let rec addCSTC i C = 
    match (i, C) with
    | _                     -> (CSTC ((int32)(System.BitConverter.ToInt16((System.BitConverter.GetBytes(char(i))),0)))) :: C

let encoding = System.Text.ASCIIEncoding();


// let rec addCSTS i C = 
    // match (i, C) with
    // | _                     -> (CSTS ((int32)(System.BitConverter.ToString (System.Text.Encoding.Default.GetBytes(i+""))))) :: C

let rec addCSTS i C = 
    match (i, C) with
    | _                     -> (CSTS ((int32)(System.BitConverter.ToInt16 (System.ReadOnlySpan(System.Text.Encoding.Default.GetBytes(i+"")))))) :: C

(* 环境操作 *)

type 'data Env = (string * 'data) list

let rec lookup env x = 
    match env with 
    | []         -> failwith (x + " not found")
    | (y, v)::yr -> if x=y then v else lookup yr x


type Var = 
    | Glovar of int      // 栈中的绝对地址   
    | Locvar of int      // 栈底的相对地址


// 变量环境追踪全局变量和局部变量
type VarEnv = (Var * typ) Env * int

// 函数环境
type Paramdecs = (typ * string) list
type FunEnv = (label * typ option * Paramdecs) Env
type LabEnv = label list


let rec exit labs = 
    match labs with
    | lab :: tr -> lab
    | []        -> failwith "error, unknow break, please check your code"


let rec exitOne labs =
    match labs with
    | lab :: tr -> tr
    | []        -> []


// 绑定varEnv中声明的变量并生成代码进行分配
let allocate (kind : int -> Var) (typ, x) (varEnv : VarEnv) : VarEnv * instrs list =
    let (env, fdepth) = varEnv 
    match typ with
    | TypArray (TypArray _, _) -> failwith "allocate: arrays of arrays not permitted"
    | TypArray (t, Some i) ->
      let newEnv = ((x, (kind (fdepth+i), typ)) :: env, fdepth+i+1)
      let code = [INCSP i; GETSP; CSTI (i-1); SUB]
      (newEnv, code)
    | _ -> 
      let newEnv = ((x, (kind (fdepth), typ)) :: env, fdepth+1)
      let code = [INCSP 1]
      (newEnv, code)


// 绑定varEnv中声明的参数
let bindParam (env, fdepth) (typ, x) : VarEnv = 
    ((x, (Locvar fdepth, typ)) :: env, fdepth+1);

let bindParams paras (env, fdepth) : VarEnv = 
    List.fold bindParam (env, fdepth) paras;


(* ------------------------------------------------------------------- *)

(* Build environments for global variables and global functions *)

// let makeGlobalEnvs(topdecs : topDec list) : VarEnv * FunEnv * instrs list = 
//     let rec addv decs varEnv funEnv = 
//         match decs with 
//         | [] -> (varEnv, funEnv, [])
//         | dec::decr -> 
//           match dec with
//           | VarDec (typ, x) ->
//             let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
//             let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
//             (varEnvr, funEnvr, code1 @ coder)
//           | VarDecAsg (typ, x, e) ->
//             let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
//             let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
//             (varEnvr, funEnvr, code1 @ (cAccess (AccVar(x)) varEnvr funEnvr []))
//           | FunDec (tyOpt, f, xs, body) ->
//             addv decr varEnv ((f, (newLabel(), tyOpt, xs)) :: funEnv)
//     addv topdecs ([], 0) []
    
(* ------------------------------------------------------------------- *)


let rec cStmt stmt (varEnv : VarEnv) (funEnv : FunEnv) (lablist : LabEnv) (C : instrs list) : instrs list = 
    match stmt with
    | For(e1, e2, e3, stmt) ->
      let label = newLabel()
      let labStart = newLabel()
      let labEnd = newLabel()
      let lablist = labEnd :: label :: lablist
      let cend = Label labEnd :: C
      let (jumptest, C1) =
        makeJump (cExpr e2 varEnv funEnv lablist (IFNZRO labStart :: cend))
      let C2 = Label label :: cExpr e3 varEnv funEnv lablist (addINCSP -1 C1)
      let C3 = cStmt stmt varEnv funEnv lablist C2
      cExpr e1 varEnv funEnv lablist (addINCSP -1 (addJump jumptest (Label labStart :: C3)))
    | DoWhile(body, e)    ->
      let labBegin = newLabel()
      let (labEnd, c) = addLabel C
      let lablist = labEnd :: labBegin :: lablist
      let C1 = 
        cExpr e varEnv funEnv lablist (IFNZRO labBegin :: c)
      Label labBegin :: cStmt body varEnv funEnv lablist C1
      // let (jumptest, C1) =
        // makeJump (cExpr e varEnv funEnv lablist (IFNZRO labbegin :: C))
      // addJump jumptest (Label labbegin :: cStmt body varEnv funEnv lablist C1)
    | Break ->
      let label = exit lablist
      addGOTO label C
    | Continue ->
      let label = exitOne lablist
      let labelBegin = exit label
      addGOTO labelBegin C
    | Switch(e, body) ->
      let (labEnd, C1) = addLabel C
      let lablist = labEnd :: lablist
      let rec allCase case = 
        match case with
        | [Case(n, content)] ->
          let (label, C2) = addLabel(cStmt content varEnv funEnv lablist C1)
          let (label2, C3) = addLabel(cExpr (BinaryPrim("==", e, n)) varEnv funEnv lablist (IFZERO labEnd :: C2))
          (label, label2, C3)
        | Case(n, content) :: tr ->
          let (labelNextBody, labelNext, C2) = allCase tr
          let (label, C3) = addLabel(cStmt content varEnv funEnv lablist (addGOTO labelNextBody C2))
          let (label2, C4) = addLabel(cExpr (BinaryPrim("==", e, n)) varEnv funEnv lablist (IFZERO labelNext :: C3))
          (label, label2, C4)
        | [Default(last)] ->
          let (label, C2) = addLabel(cStmt last varEnv funEnv lablist C1)
          let (label2, C3) = addLabel(cExpr (BinaryPrim("==", e, e)) varEnv funEnv lablist (IFZERO labEnd :: C2 ))
          (label, label2, C3)
        | Default(last) :: tr ->
          let (labelNextBody, labelNext, C2) = allCase tr
          let (label, C3) = addLabel(cStmt last varEnv funEnv lablist (addGOTO labelNextBody C2))
          let (label2, C4) = addLabel(cExpr (BinaryPrim("==", e, e)) varEnv funEnv lablist (IFZERO labelNext :: C3 ))
          (label, label2, C4)
      let (label, label2, C2) =
        allCase body
      C2
    | Case(n, content) ->
      C
    | Default(body) ->
      C
    | Loop(body) ->
      let labelBegin = newLabel()
      let (labEnd, c) = addLabel C
      let lablist = labEnd :: labelBegin :: lablist
      let e = CstInt 1
      let (jumptest, C1) =
        makeJump (cExpr e varEnv funEnv lablist (IFNZRO labelBegin :: c) )
      addJump jumptest (Label labelBegin :: cStmt body varEnv funEnv lablist C1)
    | Range(e, l, r, body) ->
      let rec temp start =
        match start with
        |Access (c) -> c
      let ass = Assign (temp e, l)
      let decide = BinaryPrim("<", Access(temp e), r)
      let nextOp = Assign(temp e, BinaryPrim("+", Access(temp e), CstInt 1))
      cStmt (For (ass, decide, nextOp, body)) varEnv funEnv lablist C
    // | Try(stmt, catch) ->
    //   let exps = [Exception "Excption because of math compute"]
    //   let rec lookupExp e (es : Exception list) expdepth = 
    //     match es with
    //     | head :: tail -> if e = head then expdepth else lookupExp e tail expdepth+1
    //     | [] -> -1
    //   let (labEnd, C1) = addLabel C
    //   let lablist = labEnd :: lablist
    //   let (env, fdepth) = varEnv
    //   let varEnv = (env, fdepth+3*catch.length)
    //   let (trys, varEnv) = tryStmt


    | If(e, stmt1, stmt2) -> 
      let (jumpend, C1) = makeJump C
      let (labelse, C2) = addLabel (cStmt stmt2 varEnv funEnv lablist C1)
      cExpr e varEnv funEnv lablist (IFZERO labelse 
       :: cStmt stmt1 varEnv funEnv lablist (addJump jumpend C2))
    | While(e, body) ->
      let labBegin = newLabel()

      // 以下两行实现break
      let (labEnd, c) = addLabel C
      let lablist = labEnd :: labBegin :: lablist

      let (jumptest, C1) = 
           makeJump (cExpr e varEnv funEnv lablist (IFNZRO labBegin :: c))
      addJump jumptest (Label labBegin :: cStmt body varEnv funEnv lablist C1)
    | Expr e -> 
      cExpr e varEnv funEnv lablist (addINCSP -1 C) 
    | Block stmts -> 
      let rec pass1 stmts ((_, fdepth) as varEnv) =
          match stmts with 
          | []     -> ([], fdepth)
          | s1::sr ->
            let (_, varEnv1) as res1 = bStmtordec s1 varEnv
            let (resr, fdepthr) = pass1 sr varEnv1 
            (res1 :: resr, fdepthr) 
      let (stmtsback, fdepthend) = pass1 stmts varEnv
      let rec pass2 pairs C = 
          match pairs with 
          | [] -> C
          | (BDec code,  varEnv) :: sr -> code @ pass2 sr C
          | (BStmt stmt, varEnv) :: sr -> cStmt stmt varEnv funEnv lablist (pass2 sr C)
      pass2 stmtsback (addINCSP(snd varEnv - fdepthend) C)
    | Return None -> 
      RET (snd varEnv - 1) :: deadcode C
    | Return (Some e) -> 
      cExpr e varEnv funEnv lablist (RET (snd varEnv) :: deadcode C)

and bStmtordec stmtOrDec varEnv : bstmtordec * VarEnv =
    match stmtOrDec with 
    | Stmt stmt    ->
      (BStmt stmt, varEnv) 
    | Dec (typ, x) ->
      let (varEnv1, code) = allocate Locvar (typ, x) varEnv 
      (BDec code, varEnv1)
    | DecAsg (typ, x, e) ->
      let (varEnv1, code) = allocate Locvar (typ, x) varEnv
      (BDec (cAccess (AccVar(x)) varEnv1 [] [] (cExpr e varEnv1 [] [] (STI :: (addINCSP -1 code)))), varEnv1)



and cExpr (e : expr) (varEnv : VarEnv) (funEnv : FunEnv) (lablist : LabEnv) (C : instrs list) : instrs list =
    match e with
    | Access acc     -> cAccess acc varEnv funEnv lablist (LDI :: C)
    | Assign(acc, e) -> cAccess acc varEnv funEnv lablist (cExpr e varEnv funEnv lablist (STI :: C))
    | CstInt i       -> addCST i C
    | CstFloat i     -> addCSTF i C
    | CstChar i      -> addCSTC i C
    | CstString i    -> addCSTS i C
    | Addr acc       -> cAccess acc varEnv funEnv lablist C
    | UnaryPrim(ope, e1) ->
      let rec temp start =
                   match start with
                   | Access (c) -> c
      cExpr e1 varEnv funEnv lablist
          (match ope with
           | "!"      -> addNOT C
           | "printi" -> PRINTI :: C
           | "printc" -> PRINTC :: C
           | "sleep"  -> Sleep :: C
           | "I++"    ->
                let ass = Assign (temp e1, BinaryPrim ("+", Access (temp e1), CstInt 1))
                cExpr ass varEnv funEnv lablist (addINCSP -1 C)
           | "I--"    ->
                let ass = Assign (temp e1, BinaryPrim ("-", Access (temp e1), CstInt 1))
                cExpr ass varEnv funEnv lablist (addINCSP -1 C)
           | "++I"    ->
                let ass = Assign (temp e1, BinaryPrim ("+", Access (temp e1), CstInt 1))
                let C1 = cExpr ass varEnv funEnv lablist C
                CSTI 1 :: ADD :: (addINCSP -1 C1)           
           | "--I"    ->
                let ass = Assign (temp e1, BinaryPrim ("-", Access (temp e1), CstInt 1))
                let C1 = cExpr ass varEnv funEnv lablist C
                CSTI 1 :: SUB :: (addINCSP -1 C1)              
           | _        -> failwith "unknown unary, please check it")
    | BinaryPrim(ope, e1, e2) ->
      cExpr e1 varEnv funEnv lablist
        (cExpr e2 varEnv funEnv lablist
           (match ope with
            | "*"   -> MUL  :: C
            | "+"   -> ADD  :: C
            | "-"   -> SUB  :: C
            | "/"   -> DIV  :: C
            | "%"   -> MOD  :: C
            | "=="  -> EQ   :: C
            | "!="  -> EQ   :: addNOT C
            | "<"   -> LT   :: C
            | ">="  -> LT   :: addNOT C
            | ">"   -> SWAP :: LT :: C
            | "<="  -> SWAP :: LT :: addNOT C
            | _     -> failwith "unknown binary, please check it"))
    | TernaryPrim(ope, e1, e2)  ->
      let (jumpend, C1) = makeJump C
      let (labelse, C2) = addLabel (cExpr e2 varEnv funEnv lablist C1)
      cExpr ope varEnv funEnv lablist (IFZERO labelse :: cExpr e1 varEnv funEnv lablist (addJump jumpend C2))
    | Andalso(e1, e2) ->
      match C with
      | IFZERO lab :: _ ->
         cExpr e1 varEnv funEnv lablist (IFZERO lab :: cExpr e2 varEnv funEnv lablist C)
      | IFNZRO labthen :: C1 -> 
        let (labelse, C2) = addLabel C1
        cExpr e1 varEnv funEnv lablist
           (IFZERO labelse 
              :: cExpr e2 varEnv funEnv lablist (IFNZRO labthen :: C2))
      | _ ->
        let (jumpend,  C1) = makeJump C
        let (labfalse, C2) = addLabel (addCST 0 C1)
        cExpr e1 varEnv funEnv lablist
          (IFZERO labfalse 
             :: cExpr e2 varEnv funEnv lablist (addJump jumpend C2))
    | Orelse(e1, e2) -> 
      match C with
      | IFNZRO lab :: _ -> 
        cExpr e1 varEnv funEnv lablist (IFNZRO lab :: cExpr e2 varEnv funEnv lablist C)
      | IFZERO labthen :: C1 ->
        let(labelse, C2) = addLabel C1
        cExpr e1 varEnv funEnv lablist
           (IFNZRO labelse :: cExpr e2 varEnv funEnv lablist
             (IFZERO labthen :: C2))
      | _ ->
        let (jumpend, C1) = makeJump C
        let (labtrue, C2) = addLabel(addCST 1 C1)
        cExpr e1 varEnv funEnv lablist
           (IFNZRO labtrue 
             :: cExpr e2 varEnv funEnv lablist (addJump jumpend C2))
    | Call(f, es) -> callfun f es varEnv funEnv lablist C


// 构建全局变量和全局函数的环境
and makeGlobalEnvs(topdecs : topDec list) : VarEnv * FunEnv * instrs list = 
    let rec addv decs varEnv funEnv = 
        match decs with 
        | [] -> (varEnv, funEnv, [])
        | dec::decr -> 
          match dec with
          | VarDec (typ, x) ->
            let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
            let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
            (varEnvr, funEnvr, code1 @ coder)
          | VarDecAsg (typ, x, e) ->
            let (varEnv1, code1) = allocate Glovar (typ, x) varEnv
            let (varEnvr, funEnvr, coder) = addv decr varEnv1 funEnv
            (varEnvr, funEnvr, code1 @ (cAccess (AccVar(x)) varEnvr funEnvr [] (cExpr e varEnvr funEnvr [] (STI :: (addINCSP -1 coder)))))
          | FunDec (tyOpt, f, xs, body) ->
            addv decr varEnv ((f, (newLabel(), tyOpt, xs)) :: funEnv)
    addv topdecs ([], 0) []


// 生成访问变量、解引用指针或索引数组的代码
and cAccess access varEnv funEnv lablist C = 
    match access with 
    | AccVar x   ->
      match lookup (fst varEnv) x with
      | Glovar addr, _ -> addCST addr C
      | Locvar addr, _ -> GETBP :: addCST addr (ADD :: C)
    | AccDeref e ->
      cExpr e varEnv funEnv lablist C
    | AccIndex(acc, idx) ->
      cAccess acc varEnv funEnv lablist (LDI :: cExpr idx varEnv funEnv lablist (ADD :: C))


// 多个参数
and cExprs es varEnv funEnv lablist C = 
    match es with 
    | []     -> C
    | e1::er -> cExpr e1 varEnv funEnv lablist (cExprs er varEnv funEnv lablist C)


// 参数es 调用函数f
and callfun f es varEnv funEnv lablist C : instrs list =
    let (labf, tyOpt, paramdecs) = lookup funEnv f
    let argc = List.length es
    if argc = List.length paramdecs then
      cExprs es varEnv funEnv lablist (makeCall argc labf C)
    else
      failwith (f + ": parameter/argument mismatch")


// 编译一个完成的程序  包括全局变量、调用main、函数等
let cProgram (Prog topdecs) : instrs list = 
    let _ = resetLabels ()
    let ((globalVarEnv, _), funEnv, globalInit) = makeGlobalEnvs topdecs
    let compilefun (tyOpt, f, xs, body) =
        let (labf, _, paras) = lookup funEnv f
        let (envf, fdepthf) = bindParams paras (globalVarEnv, 0)
        let C0 = [RET (List.length paras-1)]
        let code = cStmt body (envf, fdepthf) funEnv [] C0
        Label labf :: code
    let functions = 
        List.choose (function 
                         | FunDec (rTy, name, argTy, body) 
                                    -> Some (compilefun (rTy, name, argTy, body))
                         | VarDec _ -> None)
                         topdecs
    let (mainlab, _, mainparams) = lookup funEnv "main"
    let argc = List.length mainparams
    globalInit 
    @ [LDARGS argc; CALL(argc, mainlab); STOP] 
    @ List.concat functions


// 在抽象语法树中编译程序并将其写入文件，返回程序的指令集列表
let intsToFile (inss : int list) (fname : string) = 
    File.WriteAllText(fname, String.concat " " (List.map string inss))

let contCompileToFile program fname = 
    let instrs   = cProgram program 
    let bytecode = code2ints instrs
    intsToFile bytecode fname; instrs

