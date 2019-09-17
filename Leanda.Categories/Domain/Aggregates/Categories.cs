using CQRSlite.Domain;
using Leanda.Categories.Domain.Events;
using Leanda.Categories.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Leanda.Categories.Domain
{
    public enum FolderStatus
	{
		Created = 1,
		Processing,
		Processed,
        Failed
	}

	public class Categories : AggregateRoot
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

        public List<TreeNode> Nodes { get; protected set; }


        private void Apply(CategoriesUpdated e)
        {
            if(CreatedDateTime == DateTime.MinValue)
            {
                CreatedDateTime = e.TimeStamp;
            }
            UpdatedBy = e.UserId;
            UpdatedDateTime = e.TimeStamp;

            if(e.ParentId == null)
            {
                Nodes = e.Nodes;
            }
            else
            {
                // TODO: find node by parentId and replace children
            }
		}

        protected Categories()
        {
            CreatedDateTime = DateTime.MinValue;
            Nodes = new List<TreeNode>();
        }

        public Categories(Guid id, Guid correlationId, Guid userId, Guid? parentId, string name, Guid sessionId, FolderStatus status = FolderStatus.Created)
        {
            Id = id;
            ApplyChange(new FolderCreated(Id, correlationId, userId, parentId, name, sessionId, status));
        }

        public void Rename(Guid userId, string newName)
        {
            ApplyChange(new FolderNameChanged(Id, userId, Name, newName));
        }

        public void ChangeParent(Guid userId, Guid? newParentId, Guid sessionId)
        {
            ApplyChange(new FolderMoved(Id, userId, ParentId, newParentId, sessionId));
        }

        public void Delete(Guid userId, Guid correlationId, bool force = false)
        {
            ApplyChange(new FolderDeleted(Id, correlationId, userId, force));
        }

		public void ChangeStatus(Guid userId, FolderStatus status)
		{
			ApplyChange(new StatusChanged(Id, userId, status));
		}

        public void GrantAccess(Guid userId, AccessPermissions accessPermissions)
        {
            ApplyChange(new PermissionsChanged(Id, userId, accessPermissions));
        }
    }
}
