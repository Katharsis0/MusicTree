using Microsoft.EntityFrameworkCore;
using MusicTree.Models.Entities;
using MusicTree.Models.DTOs;
using MusicTree.Models;
using System.Linq.Expressions;

namespace MusicTree.Repositories
{
    public class ArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        #region Core CRUD Operations

        public async Task<Artist> AddAsync(Artist artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task<Artist?> GetByIdAsync(string id)
        {
            return await _context.Artists
                .Include(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre)
                .Include(a => a.ArtistSubgenres)
                    .ThenInclude(asg => asg.Genre)
                        .ThenInclude(g => g.ParentGenre)
                .Include(a => a.Members)
                .Include(a => a.Albums)
                .Include(a => a.Comments.Where(c => c.IsActive))
                .Include(a => a.PhotoGallery.Where(p => p.IsActive))
                .Include(a => a.Events.Where(e => e.IsActive))
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        
        public async Task<IEnumerable<Artist>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Artists.AsQueryable();
    
            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query
                .Include(a => a.ArtistGenres)
                .Include(a => a.ArtistSubgenres)
                .Include(a => a.Members)
                .Include(a => a.Albums)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Artist?> GetByNameAsync(string name)
        {
            return await _context.Artists
                .FirstOrDefaultAsync(a => a.Name == name);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Artists.AnyAsync(a => a.Name == name);
        }

        public async Task<PagedResult<Artist>> GetPagedAsync(
            Expression<Func<Artist, bool>> filter,
            Expression<Func<Artist, object>> sortBy,
            bool descending,
            int pageNumber,
            int pageSize)
        {
            var query = _context.Artists
                .Include(a => a.ArtistGenres)
                .Include(a => a.ArtistSubgenres)
                .Include(a => a.Members)
                .Include(a => a.Albums)
                .Where(filter);

            var totalCount = await query.CountAsync();

            if (descending)
            {
                query = query.OrderByDescending(sortBy);
            }
            else
            {
                query = query.OrderBy(sortBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Artist>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> CountAsync(Expression<Func<Artist, bool>> filter)
        {
            return await _context.Artists.CountAsync(filter);
        }

        public async Task<Artist> UpdateAsync(Artist artist)
        {
            _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task<Dictionary<string, int>> GetArtistCountByCountryAsync()
        {
            return await _context.Artists
                .Where(a => a.IsActive)
                .GroupBy(a => a.OriginCountry)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Country, x => x.Count);
        }

        #endregion

        #region Genre Association Methods

        public async Task<ArtistGenre?> GetArtistGenreAssociationAsync(string artistId, string genreId)
        {
            return await _context.Set<ArtistGenre>()
                .FirstOrDefaultAsync(ag => ag.ArtistId == artistId && ag.GenreId == genreId);
        }

        public async Task AddArtistGenreAssociationAsync(string artistId, string genreId, float influenceCoefficient)
        {
            var association = new ArtistGenre
            {
                ArtistId = artistId,
                GenreId = genreId,
                InfluenceCoefficient = influenceCoefficient
            };

            await _context.Set<ArtistGenre>().AddAsync(association);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveArtistGenreAssociationAsync(string artistId, string genreId)
        {
            var association = await GetArtistGenreAssociationAsync(artistId, genreId);
            if (association == null) return false;

            _context.Set<ArtistGenre>().Remove(association);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Genre>> GetArtistGenresAsync(string artistId)
        {
            return await _context.Set<ArtistGenre>()
                .Where(ag => ag.ArtistId == artistId)
                .Include(ag => ag.Genre)
                .Select(ag => ag.Genre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Artist>> GetArtistsByGenreAsync(string genreId)
        {
            return await _context.Set<ArtistGenre>()
                .Where(ag => ag.GenreId == genreId)
                .Include(ag => ag.Artist)
                .Select(ag => ag.Artist)
                .ToListAsync();
        }

        #endregion

        #region Subgenre Association Methods

        public async Task<ArtistSubgenre?> GetArtistSubgenreAssociationAsync(string artistId, string genreId)
        {
            return await _context.Set<ArtistSubgenre>()
                .FirstOrDefaultAsync(asg => asg.ArtistId == artistId && asg.GenreId == genreId);
        }

        public async Task AddArtistSubgenreAssociationAsync(string artistId, string genreId, float influenceCoefficient)
        {
            var association = new ArtistSubgenre
            {
                ArtistId = artistId,
                GenreId = genreId,
                InfluenceCoefficient = influenceCoefficient
            };

            await _context.Set<ArtistSubgenre>().AddAsync(association);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveArtistSubgenreAssociationAsync(string artistId, string genreId)
        {
            var association = await GetArtistSubgenreAssociationAsync(artistId, genreId);
            if (association == null) return false;

            _context.Set<ArtistSubgenre>().Remove(association);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Genre>> GetArtistSubgenresAsync(string artistId)
        {
            return await _context.Set<ArtistSubgenre>()
                .Where(asg => asg.ArtistId == artistId)
                .Include(asg => asg.Genre)
                    .ThenInclude(g => g.ParentGenre)
                .Select(asg => asg.Genre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Artist>> GetArtistsBySubgenreAsync(string genreId)
        {
            return await _context.Set<ArtistSubgenre>()
                .Where(asg => asg.GenreId == genreId)
                .Include(asg => asg.Artist)
                .Select(asg => asg.Artist)
                .ToListAsync();
        }

        #endregion

        #region Artist Member Methods

        public async Task AddArtistMemberAsync(string artistId, ArtistCreateDto.ArtistMemberDto memberDto)
        {
            var member = new ArtistMember
            {
                ArtistId = artistId,
                FullName = memberDto.FullName,
                Instrument = memberDto.Instrument,
                ActivityPeriod = memberDto.ActivityPeriod,
                IsActive = memberDto.IsActive
            };

            await _context.Set<ArtistMember>().AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ArtistMember>> GetArtistMembersAsync(string artistId, bool includeInactive = false)
        {
            var query = _context.Set<ArtistMember>()
                .Where(am => am.ArtistId == artistId);

            if (!includeInactive)
            {
                query = query.Where(am => am.IsActive);
            }

            return await query
                .OrderBy(am => am.FullName)
                .ToListAsync();
        }

        public async Task<ArtistMember?> GetArtistMemberByIdAsync(string memberId)
        {
            return await _context.Set<ArtistMember>()
                .Include(am => am.Artist)
                .FirstOrDefaultAsync(am => am.Id == memberId);
        }

        public async Task<bool> UpdateArtistMemberAsync(ArtistMember member)
        {
            try
            {
                _context.Set<ArtistMember>().Update(member);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveArtistMemberAsync(string memberId)
        {
            var member = await GetArtistMemberByIdAsync(memberId);
            if (member == null) return false;

            member.IsActive = false;
            return await UpdateArtistMemberAsync(member);
        }

        #endregion

        #region Album Methods

        public async Task AddAlbumAsync(string artistId, ArtistCreateDto.AlbumDto albumDto, string? coverImageUrl)
        {
            var album = new Album
            {
                ArtistId = artistId,
                Title = albumDto.Title,
                ReleaseDate = albumDto.ReleaseDate,
                CoverImageUrl = coverImageUrl,
                DurationSeconds = albumDto.DurationSeconds
            };

            // Set the album ID using the artist ID
            album.SetId(artistId);

            await _context.Set<Album>().AddAsync(album);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Album>> GetArtistAlbumsAsync(string artistId, bool includeInactive = false)
        {
            var query = _context.Set<Album>()
                .Where(a => a.ArtistId == artistId);

            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query
                .OrderBy(a => a.ReleaseDate)
                .ToListAsync();
        }

        public async Task<Album?> GetAlbumByIdAsync(string albumId)
        {
            return await _context.Set<Album>()
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(a => a.Id == albumId);
        }

        public async Task<bool> UpdateAlbumAsync(Album album)
        {
            try
            {
                _context.Set<Album>().Update(album);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveAlbumAsync(string albumId)
        {
            var album = await GetAlbumByIdAsync(albumId);
            if (album == null) return false;

            album.IsActive = false;
            return await UpdateAlbumAsync(album);
        }

        #endregion

        #region Comment Methods

        public async Task AddCommentAsync(string artistId, string content, string authorName)
        {
            var comment = new Comment
            {
                ArtistId = artistId,
                Content = content,
                AuthorName = authorName
            };

            await _context.Set<Comment>().AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetArtistCommentsAsync(string artistId, bool includeInactive = false)
        {
            var query = _context.Set<Comment>()
                .Where(c => c.ArtistId == artistId);

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(string commentId)
        {
            return await _context.Set<Comment>()
                .Include(c => c.Artist)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<bool> RemoveCommentAsync(string commentId)
        {
            var comment = await GetCommentByIdAsync(commentId);
            if (comment == null) return false;

            comment.IsActive = false;
            _context.Set<Comment>().Update(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Photo Gallery Methods

        public async Task AddPhotoAsync(string artistId, string imageUrl, string? caption = null)
        {
            var photo = new Photo
            {
                ArtistId = artistId,
                ImageUrl = imageUrl,
                Caption = caption
            };

            await _context.Set<Photo>().AddAsync(photo);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Photo>> GetArtistPhotosAsync(string artistId, bool includeInactive = false)
        {
            var query = _context.Set<Photo>()
                .Where(p => p.ArtistId == artistId);

            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Photo?> GetPhotoByIdAsync(string photoId)
        {
            return await _context.Set<Photo>()
                .Include(p => p.Artist)
                .FirstOrDefaultAsync(p => p.Id == photoId);
        }

        public async Task<bool> RemovePhotoAsync(string photoId)
        {
            var photo = await GetPhotoByIdAsync(photoId);
            if (photo == null) return false;

            photo.IsActive = false;
            _context.Set<Photo>().Update(photo);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Event Methods

        public async Task AddEventAsync(string artistId, string title, string description, string venue, 
            string city, string country, DateTime eventDate)
        {
            var eventEntity = new Event
            {
                ArtistId = artistId,
                Title = title,
                Description = description,
                Venue = venue,
                City = city,
                Country = country,
                EventDate = eventDate
            };

            await _context.Set<Event>().AddAsync(eventEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Event>> GetArtistEventsAsync(string artistId, bool includeInactive = false, bool futureOnly = false)
        {
            var query = _context.Set<Event>()
                .Where(e => e.ArtistId == artistId);

            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }

            if (futureOnly)
            {
                query = query.Where(e => e.EventDate >= DateTime.UtcNow);
            }

            return await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(string eventId)
        {
            return await _context.Set<Event>()
                .Include(e => e.Artist)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<bool> UpdateEventAsync(Event eventEntity)
        {
            try
            {
                _context.Set<Event>().Update(eventEntity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveEventAsync(string eventId)
        {
            var eventEntity = await GetEventByIdAsync(eventId);
            if (eventEntity == null) return false;

            eventEntity.IsActive = false;
            return await UpdateEventAsync(eventEntity);
        }

        #endregion

        #region Initialization Methods (Auto-generated collections)

        public async Task InitializeCommentThreadAsync(string artistId)
        {
            // Initialize empty comment thread - no action needed
            // The thread exists by virtue of the artist existing
            await Task.CompletedTask;
        }

        public async Task InitializePhotoGalleryAsync(string artistId)
        {
            // Initialize empty photo gallery - no action needed
            // The gallery exists by virtue of the artist existing
            await Task.CompletedTask;
        }

        public async Task InitializeEventsCalendarAsync(string artistId)
        {
            // Initialize empty events calendar - no action needed
            // The calendar exists by virtue of the artist existing
            await Task.CompletedTask;
        }

        #endregion

        #region Analytics and Statistics Methods

        public async Task<Dictionary<string, object>> GetArtistStatisticsAsync(string artistId)
        {
            var artist = await GetByIdAsync(artistId);
            if (artist == null) return new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["GenreCount"] = artist.ArtistGenres.Count,
                ["SubgenreCount"] = artist.ArtistSubgenres.Count,
                ["AlbumCount"] = artist.Albums.Count(a => a.IsActive),
                ["ActiveMemberCount"] = artist.Members.Count(m => m.IsActive),
                ["CommentCount"] = artist.Comments.Count(c => c.IsActive),
                ["PhotoCount"] = artist.PhotoGallery.Count(p => p.IsActive),
                ["UpcomingEventCount"] = artist.Events.Count(e => e.IsActive && e.EventDate >= DateTime.UtcNow),
                ["TotalEventCount"] = artist.Events.Count(e => e.IsActive)
            };
        }

        public async Task<Dictionary<string, int>> GetArtistCountByActivityYearAsync()
        {
            var artists = await _context.Artists
                .Where(a => a.IsActive)
                .Select(a => new { a.ActivityYears })
                .ToListAsync();

            var yearCounts = new Dictionary<string, int>();

            foreach (var artist in artists)
            {
                var years = ParseActivityYears(artist.ActivityYears);
                foreach (var year in years)
                {
                    var yearKey = year.ToString();
                    yearCounts[yearKey] = yearCounts.GetValueOrDefault(yearKey, 0) + 1;
                }
            }

            return yearCounts;
        }

        public async Task<IEnumerable<Artist>> GetMostDiverseArtistsAsync(int topCount = 10)
        {
            return await _context.Artists
                .Include(a => a.ArtistGenres)
                    .ThenInclude(ag => ag.Genre)
                        .ThenInclude(g => g.Cluster)
                .Include(a => a.ArtistSubgenres)
                    .ThenInclude(asg => asg.Genre)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.ArtistGenres.Select(ag => ag.Genre.ClusterId).Distinct().Count())
                .Take(topCount)
                .ToListAsync();
        }

        #endregion

        #region Private Helper Methods

        private static List<int> ParseActivityYears(string activityYears)
        {
            var years = new List<int>();
            if (string.IsNullOrWhiteSpace(activityYears)) return years;

            var parts = activityYears.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                
                // Handle single year
                if (int.TryParse(trimmed, out int singleYear))
                {
                    years.Add(singleYear);
                    continue;
                }

                // Handle range
                if (trimmed.Contains('–'))
                {
                    var rangeParts = trimmed.Split('–');
                    if (rangeParts.Length == 2)
                    {
                        var startStr = rangeParts[0].Trim();
                        var endStr = rangeParts[1].Trim();

                        if (int.TryParse(startStr, out int startYear))
                        {
                            int endYear;
                            if (endStr.Equals("presente", StringComparison.OrdinalIgnoreCase) ||
                                endStr.Equals("present", StringComparison.OrdinalIgnoreCase))
                            {
                                endYear = DateTime.Now.Year;
                            }
                            else if (!int.TryParse(endStr, out endYear))
                            {
                                continue;
                            }

                            for (int year = startYear; year <= endYear; year++)
                            {
                                years.Add(year);
                            }
                        }
                    }
                }
            }

            return years.Distinct().ToList();
        }

        #endregion
    }
}