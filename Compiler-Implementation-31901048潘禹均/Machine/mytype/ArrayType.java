package mytype;

public class ArrayType extends basicType{
    private basicType[] value;
    private int length;

    public ArrayType(){
        value = null;
    }

    public ArrayType(basicType[] array){
        value = array;
    }

    public basicType[] getValue() {
        return value;
    }

    public void setValue(basicType[] value) {
        this.value = value;
    }
}

