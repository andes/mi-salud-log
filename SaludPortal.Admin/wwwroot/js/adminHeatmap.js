(function () {
    'use strict';

    /** @type {Record<string, { map: L.Map, heatLayer: L.HeatLayer }>} */
    const instances = {};

    window.adminHeatmap = {
        /**
         * Initialize a Leaflet heatmap inside the given element.
         * @param {string} elementId  - id of the container div
         * @param {number[][]} points - array of [lat, lon, intensity]
         */
        init(elementId, points) {
            // Dispose any existing instance for this element
            if (instances[elementId]) {
                instances[elementId].map.remove();
                delete instances[elementId];
            }

            const container = document.getElementById(elementId);
            if (!container) return;

            // Center on Neuquén province
            const map = L.map(elementId).setView([-38.95, -68.07], 7);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                maxZoom: 19
            }).addTo(map);

            const heatLayer = L.heatLayer(points, {
                radius: 25,
                blur: 15,
                maxZoom: 17,
                gradient: { 0.4: '#2196f3', 0.65: '#4caf50', 1: '#f44336' }
            }).addTo(map);

            instances[elementId] = { map, heatLayer };
        },

        /**
         * Replace the heatmap data without recreating the map.
         * @param {string} elementId  - id of the container div
         * @param {number[][]} points - array of [lat, lon, intensity]
         */
        update(elementId, points) {
            const entry = instances[elementId];
            if (!entry) return;
            entry.heatLayer.setLatLngs(points);
        },

        /**
         * Remove the map instance and free memory (call from IAsyncDisposable).
         * @param {string} elementId - id of the container div
         */
        destroy(elementId) {
            const entry = instances[elementId];
            if (!entry) return;
            entry.map.remove();
            delete instances[elementId];
        }
    };
}());
