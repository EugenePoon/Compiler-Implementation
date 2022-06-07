int main()
{
    int i;
    for(i = 0; i < 13; i++) {
        if(i % 2 == 1)
            continue;
        print i;
    }
}