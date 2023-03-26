angular.module("umbraco").controller("RedirectManagerDashboard", function($scope, RdmDashboardResource) {
	
    var vm = this;
	vm.originalUrl = "";
    vm.HomePages = [];
    vm.selectedHomePage = {};
	vm.Nodes = [];
	vm.selectedNode = {};
	vm.errors = [];
	vm.responseMessage = "";
	

    RdmDashboardResource.getHomePages().then(response => {
        vm.HomePages = response;
    });

    $scope.updateSelectedHomePage = function () {
		if (vm.selectedHomePage != null) {
			RdmDashboardResource.getNodeChildren(vm.selectedHomePage.Id).then(response => {
				vm.Nodes = response;
			});
		}
    }
	
	$scope.openNode = function (nodeId) {
		RdmDashboardResource.getNodeChildren(nodeId).then(response => {
			vm.Nodes = response;
		});
    }
	
	$scope.updateSelectedNode = function (node) {
		vm.selectedNode = node;
    }
	
	$scope.getSelectableNodeChildren = function () {
		this.openNode(vm.selectedNode.Id);
	}
	
	$scope.nodeClicked = function (node) {
		if (node.HasChildren && node.NodeType !== "sectionPage") {
			this.openNode(node.Id);
		}
		else {
			if (node.NodeType !== "section" && node.NodeType !== "subSection") {
				this.updateSelectedNode(node);
			}
		}
    }
	
	$scope.isEmptyObject = function (obj) {
		if (obj && Object.keys(obj).length === 0 && Object.getPrototypeOf(obj) === Object.prototype) {
			return true;
		}
		return false;
	}
	
	$scope.isEmptyArray = function (arr) {
		if (!Array.isArray(arr) || !arr.length) {
			return true;
		}
		return false;
	}
	
	$scope.createNewRedirect = function () {
		vm.errors = [];
		vm.responseMessage = "";
		
		this.isEmptyObject(vm.selectedHomePage) && vm.errors.push("Please select a School");
		this.isEmptyObject(vm.selectedNode) && vm.errors.push("Please select a destination");
		(vm.originalUrl == "") && vm.errors.push("Please provide the old URL");
		
		if (!this.isEmptyArray(vm.errors)) {
			return;
		}
		vm.errors = [];
		
		var newRedirectData = {
			OldUrl: vm.originalUrl,
			DestinationGuid: vm.selectedNode.Key,
		}
		
		RdmDashboardResource.registerRedirect(newRedirectData).then(response => {
			vm.responseMessage = response;
		})
		.catch (error => vm.errors.push(error.data));
    }
	
	$scope.navigateUpDirectory = function () {
		if (vm.Nodes == null || this.isEmptyArray(vm.Nodes)) {
			RdmDashboardResource.getNodeImmediateAncestors(vm.selectedNode.Id, 1).then(response => {
				vm.Nodes = response;
			});
			return;
		}

		var firstChildNode = vm.Nodes[0];
		if (firstChildNode.Level > 2) {
			RdmDashboardResource.getNodeImmediateAncestors(firstChildNode.Id, 2).then(response => {
				vm.Nodes = response;
			});
		}
	}
	
});