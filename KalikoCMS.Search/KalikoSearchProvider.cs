﻿
namespace KalikoCMS.Search {
    using System;
    using Events;
    using KalikoSearch.Core;

    public class KalikoSearchProvider : SearchProviderBase {
        private readonly Collection _collection;
        private static readonly string[] SearchFields = new[] { "title", "summary", "content", "category" };

        public KalikoSearchProvider() {
            _collection = new Collection("KalikoCMS");
        }

        public override void AddToIndex(IndexItem item) {
            string key = GetKey(item.PageId, item.LanguageId);
            var pageDocument = new PageDocument(key, item);
            _collection.AddDocument(pageDocument);

            IndexingFinished();
        }

        public override void RemoveAll() {
            _collection.RemoveAll();
        }

        public override void RemoveFromIndex(Guid pageId, int languageId) {
            string key = GetKey(pageId, languageId);
            _collection.RemoveDocument(key);
        }

        public override void IndexingFinished() {
            _collection.OptimizeIndex();
        }

        private string GetKey(Guid pageId, int languageId) {
            string key = string.Format("{0}:{1}", pageId, languageId);
            return key;
        }

        public override void Init() {
            PageFactory.PageSaved += OnPageSaved;
        }

        void OnPageSaved(object sender, PageEventArgs e) {
            IndexPage(e.Page);
        }

        public override SearchResult Search(SearchQuery query) {
            KalikoSearch.Core.SearchResult searchResult = _collection.Search(query.SearchString, SearchFields, query.MetaData, query.ReturnFromPosition, query.NumberOfHitsToReturn);

            SearchResult result = ConvertResult(searchResult);

            return result;
        }

        private SearchResult ConvertResult(KalikoSearch.Core.SearchResult searchResult) {
            var result = new SearchResult {
                NumberOfHits = searchResult.NumberOfHits, 
                SecondsTaken = searchResult.SecondsTaken
            };

            foreach (var hit in searchResult.Hits) {
                result.Hits.Add(new SearchHit {
                    Excerpt = hit.Excerpt, 
                    Path = hit.Path, 
                    Title = hit.Title, 
                    MetaData = hit.MetaData, 
                    PageId = hit.PageId
                });
            }

            return result;
        }

    }
}