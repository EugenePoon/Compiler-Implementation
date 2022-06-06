package mytype;

public class CharType extends basicType {
    private char value;

    CharType(){
        value = 0;
    }

    public CharType(char c){
        value = c;  
    }

    public char getValue() {
        return value;
    }
    public void setValue(char value) {
        this.value = value;
    }
}

