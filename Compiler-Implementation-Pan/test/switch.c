int main(){
    int i;
    int n;
    i=0;
    n=1;
    switch(n){
        case 1:{i=n+n;i=i+5;}
        case 2:{i=i*2;}
        case 3:{i=i+1;break;}
        case 5:i=i+n*n;
    }
    print i;
}