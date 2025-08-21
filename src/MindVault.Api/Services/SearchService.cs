using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Util;
using MindVault.Api.Data;
using MindVault.Api.Models;

namespace MindVault.Api.Services
{
    public class SearchService : IDisposable
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly RAMDirectory _dir;
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _writer;

        public SearchService()
        {
            _dir = new RAMDirectory();
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            _writer = new IndexWriter(_dir, indexConfig);
        }

        public void IndexNote(Note note)
        {
            var doc = new Document
            {
                new StringField("Id", note.Id.ToString(), Field.Store.YES),
                new TextField("Title", note.Title ?? "", Field.Store.YES),
                new TextField("Body", note.Body ?? "", Field.Store.NO) // Body stored optional
            };
            // remove existing doc with same id
            _writer.UpdateDocument(new Term("Id", note.Id.ToString()), doc);
            _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public List<Guid> Search(string q, int max = 20)
        {
            _writer.Commit();
            using var reader = _writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);

            var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "Title", "Body" }, _analyzer);
            var query = parser.Parse(q);

            var hits = searcher.Search(query, max).ScoreDocs;
            var results = new List<Guid>();
            foreach (var hit in hits)
            {
                var doc = searcher.Doc(hit.Doc);
                if (Guid.TryParse(doc.Get("Id"), out var id)) results.Add(id);
            }
            return results;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _analyzer?.Dispose();
            _dir?.Dispose();
        }
    }
}
