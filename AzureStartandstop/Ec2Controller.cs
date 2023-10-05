using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using AzureStartandstop;
using Microsoft.AspNetCore.Mvc;

namespace EC2ManagementAPI.Controllers
{
    [Route("api/ec2")]
    [ApiController]
    public class EC2Controller : ControllerBase
    {
        [HttpPost("get-instances")]
        public async Task<IActionResult> GetEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));
                var request = new DescribeInstancesRequest();
                var response = await ec2Client.DescribeInstancesAsync(request);
                var instances = new List<Instance>();

                foreach (var reservation in response.Reservations)
                {
                    instances.AddRange(reservation.Instances);
                }

                return Ok(instances);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("start-instances")]
        public async Task<IActionResult> StartEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));
                var instanceIds = await GetInstanceIdsAsync(ec2Client);

                var request = new StartInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StartInstancesAsync(request);

                return Ok(response.StartingInstances);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stop-instances")]
        public async Task<IActionResult> StopEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));
                var instanceIds = await GetInstanceIdsAsync(ec2Client);

                var request = new StopInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StopInstancesAsync(request);

                return Ok(response.StoppingInstances);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static async Task<List<string>> GetInstanceIdsAsync(IAmazonEC2 ec2Client)
        {
            var request = new DescribeInstancesRequest();
            var response = await ec2Client.DescribeInstancesAsync(request);
            var instanceIds = new List<string>();

            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    instanceIds.Add(instance.InstanceId);
                }
            }

            return instanceIds;
        }
    }
}
