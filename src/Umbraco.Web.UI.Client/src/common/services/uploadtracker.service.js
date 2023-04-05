/**
* @ngdoc service
* @name umbraco.services.uploadTracker
* @description a helper to keep track of uploads in progress
**/
function uploadTracker(eventsService) {

  const uploadsInProgress = [];
  const events = {};

  /**
   * @ngdoc function
   * @name umbraco.services.uploadTracker#uploadStarted
   * @methodOf umbraco.services.uploadTracker
   * @function
   *
   * @description
   * Called when an upload is started to inform listeners that an upload is in progress. This will raise the uploadTracker.uploadsInProgressChanged event.
   *
   * @param {string} entityKey The key of the entity where the upload is taking place
   */
  function uploadStarted (entityKey) {
    const uploadDetails = {
      entityKey
    };

    uploadsInProgress.push(uploadDetails);
    eventsService.emit('uploadTracker.uploadsInProgressChanged', { uploadsInProgress });
  }

  /**
   * @ngdoc function
   * @name umbraco.services.uploadTracker#uploadEnded
   * @methodOf umbraco.services.uploadTracker
   * @function
   *
   * @description
   * Called when an upload is ended to inform listeners that an upload has stopped. This will raise the uploadTracker.uploadsInProgressChanged event.
   *
   * @param {string} entityKey The key of the entity where the upload has stopped.
   */
  function uploadEnded (entityKey) {
    const index = uploadsInProgress.findIndex(upload => upload.entityKey === entityKey);
    uploadsInProgress.splice(index, 1);
    eventsService.emit('uploadTracker.uploadsInProgressChanged', { uploadsInProgress });
  }

  return {
    uploadStarted,
    uploadEnded
  };
}

angular.module('umbraco.services').factory('uploadTracker', uploadTracker);