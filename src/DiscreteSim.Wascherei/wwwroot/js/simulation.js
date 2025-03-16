document.addEventListener('DOMContentLoaded', function () {
    const simulationForm = document.getElementById('simulationForm');
    if (simulationForm) {
        simulationForm.addEventListener('submit', handleFormSubmit);
    }

    const toggleFormBtn = document.getElementById('toggleForm');
    if (toggleFormBtn) {
        toggleFormBtn.addEventListener('click', function () {
            const formContainer = document.querySelector('.form-container');
            formContainer.classList.toggle('d-none');

            const collapseText = this.querySelector('.collapse-text');
            const expandText = this.querySelector('.expand-text');
            collapseText.classList.toggle('d-none');
            expandText.classList.toggle('d-none');
        });
    }
});

async function handleFormSubmit(event) {
    event.preventDefault();

    const loadingIndicator = document.getElementById('loadingIndicator');
    const resultsContainer = document.getElementById('resultsContainer');
    loadingIndicator.classList.remove('d-none');
    resultsContainer.classList.add('d-none');

    const formData = new FormData(event.target);
    const formDataObj = {};

    formData.forEach((value, key) => {
        formDataObj[key] = !isNaN(value) && value !== '' ? Number(value) : value;
    });

    const payload = {
        parameters: {
            machinesAvailable: formDataObj.machinesAvailable,
            operatingHours: formDataObj.operatingHours,
            avgMachineUsageHours: formDataObj.avgMachineUsageHours,
            dailyCustomerBaseTarget: formDataObj.dailyCustomerBaseTarget,
            avgSpendPerStudent: formDataObj.avgSpendPerStudent,
            stdDevSpend: formDataObj.stdDevSpend,
            fixedCost: formDataObj.fixedCost,
            variableCostFactor: formDataObj.variableCostFactor,
            maintenanceCost: formDataObj.maintenanceCost,
            weekdayBoostWithDiscount: formDataObj.weekdayBoostWithDiscount,
            weekendBoostWithDiscount: formDataObj.weekendBoostWithDiscount,
            weekendBoostNoDiscount: formDataObj.weekendBoostNoDiscount
        },
        year: formDataObj.year
    };

    try {
        const response = await fetch('/simulation/run', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.error || 'Failed to run simulation');
        }

        const data = await response.json();

        loadingIndicator.classList.add('d-none');
        resultsContainer.classList.remove('d-none');

        createRevenueChart(data);
        createProfitChart(data);
        createCustomerChart(data);
        createUtilizationChart(data);
        populateSummaryTables(data);
        setupDownloadButton(data);

    } catch (error) {
        loadingIndicator.classList.add('d-none');
        alert('Error running simulation: ' + error.message);
        console.error('Simulation error:', error);
    }
}

let revenueChartInstance = null;
let profitChartInstance = null;
let customerChartInstance = null;
let utilizationChartInstance = null;

function createRevenueChart(data) {
    const standardData = data.withoutDiscounts.monthlyData.map(m => m.totalRevenue);
    const discountData = data.withDiscounts.monthlyData.map(m => m.totalRevenue);
    const labels = data.withoutDiscounts.monthlyData.map(m => m.monthName);

    const canvas = document.getElementById('revenueChart');

    if (revenueChartInstance) {
        revenueChartInstance.destroy();
    }

    revenueChartInstance = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Standard Pricing',
                    data: standardData,
                    borderColor: 'rgba(54, 162, 235, 1)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    tension: 0.1
                },
                {
                    label: 'Weekday Discounts',
                    data: discountData,
                    borderColor: 'rgba(255, 99, 132, 1)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    tension: 0.1
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Monthly Revenue Comparison'
                }
            }
        }
    });
}

function createProfitChart(data) {
    const standardData = data.withoutDiscounts.monthlyData.map(m => m.totalProfit);
    const discountData = data.withDiscounts.monthlyData.map(m => m.totalProfit);
    const labels = data.withoutDiscounts.monthlyData.map(m => m.monthName);

    const canvas = document.getElementById('profitChart');

    if (profitChartInstance) {
        profitChartInstance.destroy();
    }

    profitChartInstance = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Standard Pricing',
                    data: standardData,
                    borderColor: 'rgba(54, 162, 235, 1)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    tension: 0.1
                },
                {
                    label: 'Weekday Discounts',
                    data: discountData,
                    borderColor: 'rgba(255, 99, 132, 1)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    tension: 0.1
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Monthly Profit Comparison'
                }
            }
        }
    });
}

function createCustomerChart(data) {
    const standardData = data.withoutDiscounts.monthlyData.map(m => m.totalCustomers);
    const discountData = data.withDiscounts.monthlyData.map(m => m.totalCustomers);
    const standardLostData = data.withoutDiscounts.monthlyData.map(m => m.totalLostCustomers);
    const discountLostData = data.withDiscounts.monthlyData.map(m => m.totalLostCustomers);
    const labels = data.withoutDiscounts.monthlyData.map(m => m.monthName);

    const canvas = document.getElementById('customerChart');

    if (customerChartInstance) {
        customerChartInstance.destroy();
    }

    customerChartInstance = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Standard - Served',
                    data: standardData,
                    backgroundColor: 'rgba(54, 162, 235, 0.7)',
                },
                {
                    label: 'Discount - Served',
                    data: discountData,
                    backgroundColor: 'rgba(255, 99, 132, 0.7)',
                },
                {
                    label: 'Standard - Lost',
                    data: standardLostData,
                    backgroundColor: 'rgba(54, 162, 235, 0.3)',
                },
                {
                    label: 'Discount - Lost',
                    data: discountLostData,
                    backgroundColor: 'rgba(255, 99, 132, 0.3)',
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Monthly Customer Distribution'
                }
            },
            scales: {
                x: {
                    stacked: true,
                },
                y: {
                    stacked: true
                }
            }
        }
    });
}

function createUtilizationChart(data) {
    const standardData = data.withoutDiscounts.monthlyData.map(m => m.averageMachineUtilization * 100);
    const discountData = data.withDiscounts.monthlyData.map(m => m.averageMachineUtilization * 100);
    const labels = data.withoutDiscounts.monthlyData.map(m => m.monthName);

    const canvas = document.getElementById('utilizationChart');

    if (utilizationChartInstance) {
        utilizationChartInstance.destroy();
    }

    utilizationChartInstance = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Standard Pricing',
                    data: standardData,
                    borderColor: 'rgba(54, 162, 235, 1)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    tension: 0.1
                },
                {
                    label: 'Weekday Discounts',
                    data: discountData,
                    borderColor: 'rgba(255, 99, 132, 1)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    tension: 0.1
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: true,
                    text: 'Monthly Machine Utilization (%)'
                }
            }
        }
    });
}

function populateSummaryTables(data) {
    const standardTable = document.getElementById('standardSummaryTable');
    const discountTable = document.getElementById('discountSummaryTable');

    const headerRow = `
        <thead>
            <tr>
                <th>Metric</th>
                <th>Value</th>
            </tr>
        </thead>
    `;

    standardTable.innerHTML = headerRow;
    discountTable.innerHTML = headerRow;

    const standardBody = document.createElement('tbody');
    const discountBody = document.createElement('tbody');

    const formatNumber = (num) => new Intl.NumberFormat('en-US', {
        style: 'decimal',
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(num);

    const standardData = data.withoutDiscounts;
    const discountData = data.withDiscounts;

    standardBody.innerHTML = `
        <tr>
            <td>Total Revenue</td>
            <td>$${formatNumber(standardData.totalRevenue)}</td>
        </tr>
        <tr>
            <td>Total Costs</td>
            <td>$${formatNumber(standardData.totalCosts)}</td>
        </tr>
        <tr>
            <td>Total Profit</td>
            <td>$${formatNumber(standardData.totalProfit)}</td>
        </tr>
        <tr>
            <td>Total Customers</td>
            <td>${standardData.totalCustomers}</td>
        </tr>
        <tr>
            <td>Lost Customers</td>
            <td>${standardData.totalLostCustomers}</td>
        </tr>
        <tr>
            <td>Lost Revenue</td>
            <td>$${formatNumber(standardData.totalLostRevenue)}</td>
        </tr>
        <tr>
            <td>Average Machine Utilization</td>
            <td>${formatNumber(standardData.averageMachineUtilization * 100)}%</td>
        </tr>
    `;

    discountBody.innerHTML = `
        <tr>
            <td>Total Revenue</td>
            <td>$${formatNumber(discountData.totalRevenue)}</td>
        </tr>
        <tr>
            <td>Total Costs</td>
            <td>$${formatNumber(discountData.totalCosts)}</td>
        </tr>
        <tr>
            <td>Total Profit</td>
            <td>$${formatNumber(discountData.totalProfit)}</td>
        </tr>
        <tr>
            <td>Total Customers</td>
            <td>${discountData.totalCustomers}</td>
        </tr>
        <tr>
            <td>Lost Customers</td>
            <td>${discountData.totalLostCustomers}</td>
        </tr>
        <tr>
            <td>Lost Revenue</td>
            <td>$${formatNumber(discountData.totalLostRevenue)}</td>
        </tr>
        <tr>
            <td>Average Machine Utilization</td>
            <td>${formatNumber(discountData.averageMachineUtilization * 100)}%</td>
        </tr>
    `;

    standardTable.appendChild(standardBody);
    discountTable.appendChild(discountBody);
}

function setupDownloadButton(data) {
    const downloadButton = document.getElementById('downloadJsonBtn');
    downloadButton.onclick = function () {
        const dataStr = typeof data === 'string' ? data : JSON.stringify(data, null, 2);
        const dataBlob = new Blob([dataStr], { type: 'application/json' });
        const url = URL.createObjectURL(dataBlob);

        const link = document.createElement('a');
        link.href = url;
        link.download = 'laundromat-simulation-results.json';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };
}