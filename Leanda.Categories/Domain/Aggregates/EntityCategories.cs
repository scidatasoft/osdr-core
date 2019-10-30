using CQRSlite.Domain;
using Leanda.Categories.Domain.Events;
using Leanda.Categories.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain
{
    public class EntityCategories : AggregateRoot
    {
        /// <summary>
        /// User Id of the person who created the file
        /// </summary>
        public Guid CreatedBy { get; private set; }

        /// <summary>
        /// Date and time when file was created
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; private set; }

        /// <summary>
        /// User Id of person who changed the file last time
        /// </summary>
        public Guid UpdatedBy { get; protected set; }

        /// <summary>
        /// Date and time when file was changed last time
        /// </summary>
        public DateTimeOffset UpdatedDateTime { get; protected set; }

        public IEnumerable<Guid> CategoriesIds { get; protected set; }

        private void Apply(EntityCategoriesCreated e)
        {
            CreatedBy = e.UserId;
            CreatedDateTime = e.TimeStamp;
            UpdatedBy = e.UserId;
            UpdatedDateTime = e.TimeStamp;
            CategoriesIds = e.CategoriesIds;
        }

        protected EntityCategories()
        {
        }

        public EntityCategories(Guid id, Guid userId, List<Guid> categoriesIds)
        {
            Id = id;
            ApplyChange(new EntityCategoriesCreated(Id, userId, categoriesIds));
        }
    }
}
