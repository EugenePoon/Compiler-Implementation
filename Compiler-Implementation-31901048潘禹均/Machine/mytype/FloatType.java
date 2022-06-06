package mytype;

public class FloatType extends basicType {
    private float value;


    public FloatType(){
        this.value = 0;
    }

    public FloatType(float value){
        this.value = value;
    }

    public float getValue() {
        return value;
    }

    public void setValue(float value) {
        this.value = value;
    }
} 

