angular.module('umbraco.resources').factory('RdmDashboardResource', 

    function($http, umbRequestHelper) {

        return {

			getHomePages: function() {
				return umbRequestHelper.resourcePromise(
					$http({
						method: "GET",
						url: "backoffice/RDM/Redirects/GetAllHomePages"
					}),
					'Failed to get website home pages'
				);
			},
			
			getNodeChildren: function(nodeId) {
				return umbRequestHelper.resourcePromise(
					$http({
						method: "GET",
						params: { id: nodeId },
						url: "backoffice/RDM/Redirects/GetNodeChildren"
					}),
					'Failed to get node child pages'
				);
			},
			
			registerRedirect: function(newRedirectData) {
				return umbRequestHelper.resourcePromise(
					$http({
						method: 'POST',
						url: 'backoffice/RDM/Redirects/RegisterRedirect',
						data: newRedirectData
					}),
					"Failed to register new redirect"
				);
			},
			
			getNodeImmediateAncestors: function(nodeId, levels) {
				return umbRequestHelper.resourcePromise(
					$http({
						method: "GET",
						params: { id: nodeId, levels: levels },
						url: "backoffice/RDM/Redirects/GetNodeImmediateAncestors"
					}),
					'Failed to get node ancestor pages'
				);
			},

		}

    }
); 