package mytype;

public class StringType extends basicType {
    private String value;

    StringType(){
        value = new String("zzzzz");
    }

    public StringType(char c){
        value = String.valueOf(c);
    }
    public StringType(String c){
        value = c;
    }
    public String getValue() {
        return value;
    }

    public void setValue(char value) {
        this.value = String.valueOf(value);
    }

    public String addChar(String value){
        return this.value + value;
    }

    public String toString(){
        return value;
    }
}

