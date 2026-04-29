# 🚨 Ściągawka z ADO.NET - Najczęstsze Pułapki i Rozwiązania

## 1. DataReader (Największy zabójca połączeń)
* **Zawsze zamykaj Readera:** Jeśli odpalasz `ExecuteReaderAsync`, to dopóki go nie zamkniesz, nie wykonasz ŻADNEGO innego zapytania.
  ```csharp
  var reader = await command.ExecuteReaderAsync(ct);
  // ... czytanie ...
  await reader.CloseAsync(); // ZŁOTA ZASADA!
  ```
* **Pamiętaj o przesunięciu kursora:** Zanim pobierzesz dane, musisz wywołać `ReadAsync()`. Jeśli zapytanie zwraca jeden wiersz, nie zapomnij o tym:
  ```csharp
  await reader.ReadAsync(ct);
  var value = reader.GetInt32(0); 
  ```
* **Pułapka Nulli w `LEFT JOIN`:** Uważaj na operator logiczny przy sprawdzaniu, czy dziecko istnieje!
  ```csharp
  // DOBRZE - pomiń dodawanie, jeśli id dziecka to NULL
  if (reader.IsDBNull(2)) continue; 

  // ❌ ŹLE - "jeśli ma wypożyczenie, pomiń kod" (zabójczy wykrzyknik)
  if (!reader.IsDBNull(2)) continue; 
  ```
* **Rzutowanie typów:** Zwracaj uwagę na typy kolumn w bazie. Daty (`DATETIME`) pobieraj jako `reader.GetDateTime()`, a nie `reader.GetString()`.

---

## 2. Parametry (`SqlCommand.Parameters`)
* **Czyść parametry:** Przed każdym NOWYM zapytaniem z nowymi parametrami, wyczyść stare. Inaczej SQL rzuci błędem o zduplikowanych zmiennych.
  ```csharp
  command.Parameters.Clear(); 
  command.CommandText = "select ...";
  ```
* **Używanie tych samych parametrów:** Jeśli w `UPDATE` i `DELETE` używasz tego samego `@Id`, nie dodawaj go drugi raz przez `AddWithValue`. Wystarczy dodać raz.
* **Puste notatki (Nulle z obiektu DTO):** Jeśli wysyłasz do bazy `null` z C#, SQL nie zrozumie tego tak po prostu. Musisz użyć `DBNull.Value`.
  ```csharp
  command.Parameters.AddWithValue("@Notes", (object?)dto.Notes ?? DBNull.Value);
  ```

---

## 3. Wykonywanie Zapytań i Transakcje
* **Co do czego służy:**
   * `ExecuteReaderAsync` -> Kiedy robisz `SELECT` i spodziewasz się **wielu kolumn/wierszy**.
   * `ExecuteScalarAsync` -> Kiedy robisz `SELECT 1` lub `SELECT Id`, żeby sprawdzić istnienie i pobrać **pojedynczą wartość**. (Uwaga: sprawdza się `is null` / `is not null`).
   * `ExecuteNonQueryAsync` -> Do modyfikacji! `INSERT`, `UPDATE`, `DELETE`.
* **Cichy zabójca Commit/Rollback:** Metody transakcji są asynchroniczne! Zawsze dawaj przed nimi `await`, inaczej wysypią się w tle.
  ```csharp
  await transaction.CommitAsync(ct);
  // i w catch:
  await transaction.RollbackAsync(ct);
  ```
* **Wykonaj zanim zacommitujesz:** Jeśli napiszesz `UPDATE`, dodasz parametry i od razu zrobisz `Commit`, nic się nie zapisze. Musisz to odpalić przez `ExecuteNonQueryAsync`!

---

## 4. Architektura i Kody HTTP
* **Zgubiony Await w Kontrolerze:** Jeśli kontroler ma zwrócić `NoContent()`, upewnij się, że "czeka" na serwis.
  ```csharp
  await service.ReturnRentalAsync(id, dto, ct); // Bez await kontroler wypluje 204 za szybko!
  return NoContent();
  ```
* **Jakie kody zwracać?**
   * `200 OK` -> GET zadziałał i zwraca dane, lub PUT zwrócił zaktualizowany obiekt DTO.
   * `201 Created` -> POST zadziałał (zwraca nowo utworzony obiekt DTO).
   * `204 No Content` -> DELETE zadziałał lub PUT/POST zadziałał, ale nic nie zwracamy w body.
   * `400 Bad Request` -> Złe dane wejściowe (np. data z przeszłości, złe DTO). (Wyjątek np. `BadRequestException`).
   * `404 Not Found` -> Rekord nie istnieje w bazie. (Wyjątek np. `NotFoundException`).
   * `409 Conflict` -> Łamanie reguł biznesowych, np. próba usunięcia zwróconego filmu. (Wyjątek np. `ConflictException`).

---

## 5. Słownik przy relacjach 1 do Wielu (GET)
Żeby poprawnie zgrupować dane (np. Klient -> Wypożyczenia), użyj `Dictionary<int, DTO>`. W pętli `while`:
1. Utwórz DTO rodzica (jeśli jeszcze jest `null`).
2. Sprawdź, czy dziecko nie jest nullem (`IsDBNull`).
3. Sprawdź po ID dziecka, czy jest w słowniku. Jak nie, dodaj je.
4. Pobierz listę/kolekcję z obiektu ze słownika, dopisz szczegóły, zapisz w słowniku.
5. Na koniec po pętli: `rodzic.Kolekcja = slownik.Values;`