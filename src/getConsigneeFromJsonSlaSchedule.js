'use-strict';

/**
* Name: GetContigneeFromDatabase
* Creator: Kevin Damen
* Creation Date: 2025 July 07
* Description: Verkrijg de belgegevens persoon die dienst heeft op de meegegeven datum.
* Variables:
*   - jsonSlaSchedule : string : De JSON string van de SLA schedule.
*   - date : datetime : De datum tijd waarvan de belgegevens verkregen moeten worden.
*/

/**
* The handler function receives context and variables. It returns new variables to be used in your flow.
*
* @param {object} context - the context contains environment variables accessible as context.env.varName1
* @param {object} variables - the variables from your flow (the input parameters) accessible as variables.varName1
* @returns {object} - return values that will become available in your flow
* @throws ExecutionError
*/
exports.handler = async function(context, variables) {
    if (!variables.hasOwnProperty('jsonSlaSchedule'))
        return { error: 'jsonSlaSchedule is required' };

    if (!variables.hasOwnProperty('date'))
        return { error: 'date is required' };

    // Get the list of possible consignees from the JSON SLA schedule
    const slaSchedule = JSON.parse(variables.jsonSlaSchedule);
    const consignees = slaSchedule.Consignees || [];
    const date = new Date(variables.date);

    // Find the schedule entry for the given date
    let foundSchedule = slaSchedule.Schedule.find(entry => {
        const from = new Date(entry.From);
        const to = new Date(entry.To);
        return date >= from && date <= to;
    });

    // Get consignee details if found
    if (foundSchedule) {
        const consignee = consignees.find(c => c.Key === foundSchedule.Consignee) || null;
        return consignee;
    }

    return { error: 'No consignees available for the given date', date: variables.date };
};