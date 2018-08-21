﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog;
using EOSNewYork.EOSCore;
using EOSNewYork.EOSCore.Params;
using Newtonsoft.Json;

namespace EOSLibConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            //EOSInfo.dumpGlobal();
            //EOSInfo.dumpNameVotes();
            //EOSInfo.dumpProducers();
            EOSInfo.dumpVoters();
            //EOSInfo.dumpInfo();
            //EOSInfo.dumpProduerSchedule();
            //EOSInfo.dumpAccountInfo();
            //EOSInfo.dumpAccountBalance();
            //EOSInfo.dumpNewKeyPair();
            //EOSInfo.dumpAbiJsonToBin();
            //EOSInfo.dumpBlock();

            Console.WriteLine("Done");
            //Console.ReadLine();

        }
    }

    public static class EOSInfo
    {
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //static Uri HOST = new Uri("http://mainnet.eoscanada.com");
        //static Uri HOST = new Uri("http://dev.cryptolions.io:18888");
        static Uri HOST = new Uri("http://api.eosnewyork.io");
        //static Uri PennStationHOST = new Uri("http://pennstation.eosdocs.io:7001");

        public static void dumpNewKeyPair()
        {
            var keypair = EOSKeyManager.GenerateKeyPair();
            logger.Info("New keypair generated. Private key: {0}, Public key: {1}", keypair.PrivateKey, keypair.PublicKey);
            logger.Info(keypair.PrivateKey.Length);
            logger.Info(keypair.PublicKey.Length);
        }
        public static void dumpBlock()
        {
            string blockNumber = "100";
            var info = new EOS_Object<EOSBlock_row>(HOST).getAllObjectRecordsAsync(new EOSBlock_row.postData() { block_num_or_id = blockNumber }).Result;
            logger.Info("Block {0} recieved for block num {1}", "", blockNumber);
        }
        public static void dumpAbiJsonToBin()
        {
            string _code = "eosio.token", _action = "transfer", _memo = "";
            AbiJsonToBinArgs _args = new AbiJsonToBinArgs(){ from = "yatendra1", to = "yatendra1", quantity = "1 EOS", memo = _memo };
            var info = new EOS_Object<EOSAbiJsonToBin_row>(HOST).getAllObjectRecordsAsync(new EOSAbiJsonToBin_row.postData() { code = _code, action = _action, args = _args }).Result;
            logger.Info("For code {0}, action {1}, args {2} and memo {3} recieved bin {4}", _code, _action, _args, _memo, info.binargs);

            var info2 = new EOS_Object<EOSAbiBinToJson_row>(HOST).getAllObjectRecordsAsync(new EOSAbiBinToJson_row.postData() { code = _code, action = _action, binargs = info.binargs }).Result;
            logger.Info("Received args json {0}", JsonConvert.SerializeObject(info2.args));
        }
        public static void dumpAccountBalance()
        {
            var info = new EOS_StringArray<EOSCurrencyBalance_row>(HOST).getAllObjectRecordsAsync(new EOSCurrencyBalance_row.postData() { account = "guzdqmzqgyge", code = "everipediaiq", symbol = "IQ" }).Result;
            logger.Info("The account had {0} balance records. The 1st (and probably the only balance) is {1}", info.balances.Count, info.balances.First());
        }


        public static void dumpAccountInfo()
        {
            var info = new EOS_Object<EOSAccount_row>(HOST).getAllObjectRecordsAsync(new EOSAccount_row.postData() { account_name = "yatendra1" }).Result;
            logger.Info("{0} is currently the returned account name", info.account_name);
            string json = JsonConvert.SerializeObject(info);
            logger.Info("{0}", json);
        }


        public static void dumpInfo()
        {
            var info = new EOS_Object<EOSInfo_row>(HOST).getAllObjectRecordsAsync().Result;
            logger.Info("{0} is currently the head block producer", info.head_block_producer);
        }

        public static void dumpProduerSchedule()
        {
            var info = new EOS_Object<EOSProducerSchedule_row>(HOST).getAllObjectRecordsAsync().Result;
            foreach (var producer in info.active.producers)
            {
                logger.Info("{0}\t{1}", producer.producer_name, producer.block_signing_key);
            }
            
        }

        public static void dumpGlobal()
        {
            var globalInfo = new EOS_Table<EOSGlobal_row>(HOST).getAllTableRecordsAsync().Result;

            foreach (var global in globalInfo)
            {
                logger.Debug("total_producer_vote_weight : {0}", global.total_producer_vote_weight);
            }

        }

        public static void dumpNameVotes()
        {
            StringBuilder tsvnamebids = new StringBuilder();
            var namebids = new EOS_Table<EOSNamebids_row>(HOST).getAllTableRecordsAsync().Result;

            foreach (var namebid in namebids)
            {
                tsvnamebids.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", namebid.newname, namebid.high_bid, namebid.high_bidder, namebid.last_bid_time, namebid.last_bid_time_utc));
            }

            File.WriteAllText("namebids.txt", tsvnamebids.ToString());
        }

        public static void dumpProducers()
        {
            StringBuilder tsvproducers = new StringBuilder();
            //var producers = core.getProducersAsync().Result;

            var producers = new EOS_Table<EOSProducer_row>(HOST).getAllTableRecordsAsync().Result;
            foreach (var _producer in producers)
            {
                string line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", _producer.owner, _producer.total_votes, _producer.is_active, _producer.unpaid_blocks, _producer.url);
                tsvproducers.AppendLine(line);
                //logger.Debug(line);
            }
            logger.Debug("Write {0} records to disk", producers.Count);
            File.WriteAllText("producerReport.txt", tsvproducers.ToString());

            IEnumerable<EOSProducer_row> query = producers.OrderByDescending(producer => producer.total_votes_long);

            StringBuilder tsvTop21producers = new StringBuilder();
            StringBuilder tsvTop21producersNameOnly = new StringBuilder();
            int countTop21 = 0;
            foreach (var _producer in query)
            {
                countTop21++;
                tsvTop21producers.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", _producer.owner, _producer.total_votes, _producer.is_active, _producer.unpaid_blocks, _producer.url));
                tsvTop21producersNameOnly.AppendLine(string.Format("{0}", _producer.owner));
                if (countTop21 == 21)
                    break;
            }

            File.WriteAllText("Top21ProducerReport.txt", tsvTop21producers.ToString());
            File.WriteAllText("Top21ProducerReportNameOnly.txt", tsvTop21producersNameOnly.ToString());

        }

        public static void dumpVoters()
        {
            //List<EOSVoter_row> proxyVoters = new List<EOSVoter_row>();
            StringBuilder tsvoutput = new StringBuilder();

            //var tbl = new EOS_Table<EOSVoter_row>(PennStationHOST);
            //var tbl = new EOS_Table<EOSVoter_row>(HOST);
            //var voters = tbl.getAllTableRecordsAsync().Result;
            var voters = new EOS_Table<EOSVoter_row>(HOST).getAllTableRecordsAsync().Result;
            EOSUtil.updateProxyVotersWithProducerInfo(ref voters);

            int voted = 0;
            int producerMatchCount = 0;
            long producerstake = 0;
            long totalstake = 0;
            foreach (var voter in voters)
            {
                if (voter.producers.Count > 0)
                {
                    voted++;
                    totalstake = totalstake + voter.staked;
                    foreach (var producervote in voter.producers)
                    {
                        producerMatchCount++;
                        producerstake = producerstake + voter.staked;
                        tsvoutput.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", producervote, voter.voterDescription, voter.staked / 10000, voter.last_vote_weight_for_this_account_only, voter.last_vote_weight, voter.proxied_vote_weight, String.Join(",", voter.producers.Select(x => x.ToString()).ToArray())));
                    }
                }
            }


            Console.WriteLine(voters.Count + " Records returned");
            if (voters.Count > 0)
            {
                Console.WriteLine(voters[0].owner + " = 1st record");
                Console.WriteLine(voted + " have voted");
                Console.WriteLine(totalstake + " tokens staked ");
            }

            File.WriteAllText("report.txt", tsvoutput.ToString());
        }

    }
}
