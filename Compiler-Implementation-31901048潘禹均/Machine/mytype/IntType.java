package mytype;

public class IntType extends basicType {
    private int value;

    public IntType(){
        value = 0;
    }

    public IntType(int value){
        this.value = value;
    }

    public int getValue() {
        return value;
    }

    public void setValue(int value) {
        this.value = value;
    }


}
