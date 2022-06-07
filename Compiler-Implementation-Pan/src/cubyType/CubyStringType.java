package cubyType;

public class CubyStringType extends CubyBaseType {
    private String value;

    public CubyStringType(){
        this.value = "";
    }

    public CubyStringType(String value){
        this.value = value;
    }

    public String getValue() {
        return value;
    }

    public void setValue(String value) {
        this.value = value;
    }
}
