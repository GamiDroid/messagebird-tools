# messagebird-tools

## JSON SLA Schedule format
Below is an example of the JSON SLA schedule. 
```json
{
    "Consignees": [
        { 
            "Key": "PP",
            "Name": "Peter Parker",
            "Email": "p.parker@mail.com",
            "Phone": "+31 6 123456 789"
        },
        { 
            "Key": "SS",
            "Name": "Sue Storm",
            "Email": "s.storm@mail.com",
            "Phone": "+31 6 123456 789"
        },
        { 
            "Key": "CK",
            "Name": "Clark Kent",
            "Email": "c.kent@mail.com",
            "Phone": "+31 6 123456 789"
        },
        { 
            "Key": "BW",
            "Name": "Bruce Wayne",
            "Email": "b.wayne@mail.com",
            "Phone": "+31 6 123456 789"
        }
    ],
    "Schedule": [
        { 
            "From": "2025-06-30T00:00:00Z",
            "To": "2025-07-06T23:59:59Z",
            "Consignee": "SS"
        },
        { 
            "From": "2025-07-07T00:00:00Z",
            "To": "2025-07-13T23:59:59Z",
            "Consignee": "PP"
        },
        { 
            "From": "2025-07-14T00:00:00Z",
            "To": "2025-07-20T23:59:59Z",
            "Consignee": "CK"
        },
        { 
            "From": "2025-07-21T00:00:00Z",
            "To": "2025-07-27T23:59:59Z",
            "Consignee": "BW"
        },
        { 
            "From": "2025-07-28T00:00:00Z",
            "To": "2025-08-03T23:59:59Z",
            "Consignee": "SS"
        },
        { 
            "From": "2025-08-04T00:00:00Z",
            "To": "2025-08-10T23:59:59Z",
            "Consignee": "PP"
        },
        { 
            "From": "2025-08-11T00:00:00Z",
            "To": "2025-08-17T23:59:59Z",
            "Consignee": "CK"
        },
        { 
            "From": "2025-08-18T00:00:00Z",
            "To": "2025-08-24T23:59:59Z",
            "Consignee": "BW"
        }, 
        { 
            "From": "2025-08-25T00:00:00Z",
            "To": "2025-08-31T23:59:59Z",
            "Consignee": "SS"
        },
        { 
            "From": "2025-09-01T00:00:00Z",
            "To": "2025-09-07T23:59:59Z",
            "Consignee": "PP"
        },
        { 
            "From": "2025-09-08T00:00:00Z",
            "To": "2025-09-14T23:59:59Z",
            "Consignee": "CK"
        },
        { 
            "From": "2025-09-15T00:00:00Z",
            "To": "2025-09-21T23:59:59Z",
            "Consignee": "BW"
        },
        { 
            "From": "2025-09-22T00:00:00Z",
            "To": "2025-09-28T23:59:59Z",
            "Consignee": "SS"
        },
        { 
            "From": "2025-09-29T00:00:00Z",
            "To": "2025-10-05T23:59:59Z",
            "Consignee": "PP"
        },
        { 
            "From": "2025-10-06T00:00:00Z",
            "To": "2025-10-12T23:59:59Z",
            "Consignee": "CK"
        },
        { 
            "From": "2025-10-13T00:00:00Z",
            "To": "2025-10-19T23:59:59Z",
            "Consignee": "BW"
        }
    ]
}
```