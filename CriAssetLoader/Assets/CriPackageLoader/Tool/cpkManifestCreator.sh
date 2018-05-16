#! /bin/sh


## WriteSceme
echo "cpkname,version" > $1
for var in `find . -name "*.cpk"`
do
	cpkName=`echo $var | sed "s/\.\///g"`
	hash=`md5 $var | cut -d" " -f4`
	echo "$cpkName,$hash" >> $1
done

