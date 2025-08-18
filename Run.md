# To run the bakend api, 

dotnet build (if not build yet)

## go to the api directory

dotnet run --urls "http://localhost:5108"

check the api at: http://localhost:5108/swagger/index.html    

 




# To run the fondend web, 

npm install (if not installed yet)

## go to the web directory 

npm run dev 





# To run the NLP backend, 

## go to the nlp directory 

pip install -r requirements.txt (if not installed yet)
pip install --only-binary=:all: PyMuPDF (if not installed yet)

## run the nlp api 

uvicorn main:app --host 0.0.0.0 --port 8000

check at: http://localhost:8000/docs 